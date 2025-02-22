using EGON.WebPortal.Models;
using EGON.WebPortal.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EGON.WebPortal.Pages
{
    public class EventModel : PageModel
    {
        private readonly StorageService _storageService;

        public EventModel(StorageService storageService)
        {
            _storageService = storageService;
        }

        public List<EchelonEvent> UpcomingEvents { get; set; }

        public void OnGet()
        {
            UpcomingEvents = _storageService.GetUpcomingEvent()?.ToList() ?? new();
        }
    }
}
