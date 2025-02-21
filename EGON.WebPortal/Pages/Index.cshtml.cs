using EGON.Library.Models;
using EGON.Library.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EGON.WebPortal.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly StorageService _storageService;

        public IndexModel(ILogger<IndexModel> logger, StorageService storageService)
        {
            _logger = logger;
            _storageService = storageService;
        }
        public List<EchelonEvent> UpcomingEvents { get; set; }

        public async Task OnGet()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                UpcomingEvents = _storageService.GetUpcomingEvent()?.ToList() ?? [];
            }
        }
    }
}
