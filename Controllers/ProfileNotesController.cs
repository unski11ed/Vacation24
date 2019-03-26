using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Vacation24.Models;
using Vacation24.Models.DTO;

namespace Vacation24.Controllers
{
    public class NotesController : Controller
    {
        private INotesContext _dbContext;
        public NotesController(INotesContext dbContext)
        {
            _dbContext = dbContext;
        }

        public ActionResult Save(ProfileNoteViewModel viewModel)
        {
            if (viewModel.Id == 0)
            {
                _dbContext.ProfileNotes.Add(new ProfileNote(){
                    UserId = viewModel.UserId,
                    Content = viewModel.Content,
                    Created = DateTime.Now,
                    Updated = DateTime.Now
                });
            }
            else
            {
                var model = _dbContext.ProfileNotes.Find(viewModel.Id);

                if (model == null)
                {
                    return Json(new ResultViewModel(){
                        Status = (int)ResultStatus.Error,
                        Message = "Nieznaleziono notatki"
                    });
                }

                model.Content = viewModel.Content;
                model.Updated = DateTime.Now;

                _dbContext.Entry(model).State = EntityState.Modified;
            }

            _dbContext.SaveChanges();

            return Json(new ResultViewModel()
            {
                Status = (int)ResultStatus.Success,
                Message = "Zaktualizowano notatkę"
            });
        }

        public ActionResult Get(RequestId request)
        {
            var note = _dbContext.ProfileNotes.Where(n => n.UserId == request.Id)
                                              .FirstOrDefault();

            if (note == null)
            {
                note = new ProfileNote()
                {
                    UserId = request.Id,
                    Content = ""
                };
            }

            return Json(note);
        }
    }
}