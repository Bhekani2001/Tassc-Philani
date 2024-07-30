using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Tassc.Models;
using OfficeOpenXml;
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System;

namespace Tassc.Controllers
{
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        private void PopulateSelectLists()
        {
            ViewBag.StatusOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "In Progress", Text = "In Progress" },
                new SelectListItem { Value = "In Review", Text = "In Review" },
                new SelectListItem { Value = "On Testing", Text = "On Testing" },
                new SelectListItem { Value = "Review After Testing", Text = "Review After Testing" },
                new SelectListItem { Value = "Done", Text = "Done" }
            };
        }

        public ActionResult Index()
        {
            PopulateSelectLists();
            var tasks = db.Tasks.ToList();
            return View(tasks);
        }

        [HttpGet]
        public ActionResult AddTask()
        {
            PopulateSelectLists();
            return View();
        }

        [HttpPost]
        public JsonResult AddTask(Tasks tasks)
        {
            if (ModelState.IsValid)
            {
                tasks.Status = "Unassigned";
                db.Tasks.Add(tasks);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public JsonResult UpdateStatus(int taskKey, string status)
        {
            var task = db.Tasks.Find(taskKey);
            if (task != null)
            {
                task.Status = status;
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        public ActionResult ExportToExcel()
        {
            var tasks = db.Tasks.ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Tasks");
                worksheet.Cells["A1"].Value = "Task Name";
                worksheet.Cells["B1"].Value = "Description";
                worksheet.Cells["C1"].Value = "Status";
                worksheet.Cells["D1"].Value = "Due Date";

                var row = 2;
                foreach (var task in tasks)
                {
                    worksheet.Cells[$"A{row}"].Value = task.TaskName;
                    worksheet.Cells[$"B{row}"].Value = task.Description;
                    worksheet.Cells[$"C{row}"].Value = task.Status;
                    worksheet.Cells[$"D{row}"].Value = task.DueDate.ToString("yyyy-MM-dd");
                    row++;
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Tasks.xlsx");
            }
        }

        public ActionResult ExportToPdf()
        {
            var tasks = db.Tasks.ToList();

            using (var stream = new MemoryStream())
            {
                using (var writer = new PdfWriter(stream))
                {
                    var pdf = new PdfDocument(writer);
                    var document = new Document(pdf);

                    var table = new Table(4);
                    table.AddHeaderCell("Task Name");
                    table.AddHeaderCell("Description");
                    table.AddHeaderCell("Status");
                    table.AddHeaderCell("Due Date");

                    foreach (var task in tasks)
                    {
                        table.AddCell(task.TaskName);
                        table.AddCell(task.Description);
                        table.AddCell(task.Status);
                        table.AddCell(task.DueDate.ToString("yyyy-MM-dd"));
                    }

                    document.Add(table);
                    document.Close();

                    return File(stream.ToArray(), "application/pdf", "Tasks.pdf");
                }
            }
        }

        [HttpGet]
        public ActionResult Update(int id)
        {
            try
            {
                var task = db.Tasks.Find(id);
                if (task == null)
                {
                    return HttpNotFound();
                }
                PopulateSelectLists();
                return View(task);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, "An error occurred while retrieving the task for editing.");
            }
        }

        [HttpGet]
        public JsonResult GetTaskById(int id)
        {
            var task = db.Tasks.Find(id);
            if (task != null)
            {
                return Json(new
                {
                    success = true,
                    taskkey = task.taskkey,
                    TaskName = task.TaskName,
                    Description = task.Description,
                    Status = task.Status,
                    DueDate = task.DueDate
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = false, message = "Task not found." }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Update(Tasks task)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingTask = db.Tasks.Find(task.taskkey);
                    if (existingTask == null)
                    {
                        return HttpNotFound();
                    }

                    existingTask.TaskName = task.TaskName;
                    existingTask.Description = task.Description;
                    existingTask.Status = task.Status;
                    existingTask.DueDate = task.DueDate;

                    db.Entry(existingTask).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while updating the task. Please try again.");
                    return Json(new { success = false, message = "An error occurred while updating the task. Please try again." });
                }
            }

            return Json(new { success = false, message = "Invalid data." });
        }

        [HttpPost]
        public JsonResult DeleteTask(int id)
        {
            var task = db.Tasks.Find(id);
            if (task != null)
            {
                db.Tasks.Remove(task);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
    }
}
