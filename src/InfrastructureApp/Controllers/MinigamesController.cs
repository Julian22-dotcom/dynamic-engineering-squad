using InfrastructureApp.Models;
using InfrastructureApp.Services.Minigames;
using InfrastructureApp.ViewModels.Minigames;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InfrastructureApp.Controllers
{
    [Authorize]
    public class MinigamesController : Controller
    {
        private readonly IMinigameService _minigameService;
        private readonly IMinigameViewModelFactory _minigameViewModelFactory;
        private readonly UserManager<Users> _userManager;

        public MinigamesController(
            IMinigameService minigameService,
            IMinigameViewModelFactory minigameViewModelFactory,
            UserManager<Users> userManager)
        {
            _minigameService = minigameService;
            _minigameViewModelFactory = minigameViewModelFactory;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            return View(await _minigameViewModelFactory.CreateIndexViewModelAsync(userId));
        }

        [HttpGet]
        public async Task<IActionResult> Slots()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            return View(await _minigameViewModelFactory.CreateSlotsViewModelAsync(userId));
        }

        [HttpGet]
        public async Task<IActionResult> Matching()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            return View(await _minigameViewModelFactory.CreateMatchingViewModelAsync(userId));
        }

        [HttpGet]
        public async Task<IActionResult> Trivia()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            return View(await _minigameViewModelFactory.CreateTriviaViewModelAsync(userId));
        }

        [HttpGet]
        public async Task<IActionResult> TapRepair()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            return View(await _minigameViewModelFactory.CreateTapRepairViewModelAsync(userId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SpinSlots()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var result = await _minigameService.SpinSlotsAsync(userId);

            return Json(new GameCompletionResultViewModel
            {
                GameKey = result.GameKey,
                AwardedPoints = result.AwardedPoints,
                CurrentPoints = result.CurrentPoints,
                Symbols = result.Symbols,
                IsWinningSpin = result.IsWinningSpin,
                ResultLabel = result.ResultLabel,
                DailyPointsEarned = result.DailyPointsEarned,
                DailyPointsLimit = result.DailyPointsLimit,
                HasReachedDailyLimit = result.HasReachedDailyLimit
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteGame([FromBody] CompleteGameRequestViewModel request)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            if (request == null || !MinigameConstants.IsGenericCompletionGameKey(request.GameKey))
            {
                return BadRequest(new { message = "Invalid minigame key." });
            }

            var result = await _minigameService.CompleteGameAsync(userId, request.GameKey);

            return Json(new GameCompletionResultViewModel
            {
                GameKey = result.GameKey,
                AwardedPoints = result.AwardedPoints,
                CurrentPoints = result.CurrentPoints,
                DailyPointsEarned = result.DailyPointsEarned,
                DailyPointsLimit = result.DailyPointsLimit,
                HasReachedDailyLimit = result.HasReachedDailyLimit
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitTrivia([FromBody] SubmitTriviaRequestViewModel request)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            try
            {
                var result = await _minigameService.SubmitTriviaAnswerAsync(
                    userId,
                    new TriviaAnswerSubmission
                    {
                        QuestionId = request?.QuestionId ?? string.Empty,
                        SelectedOptionKey = request?.SelectedOptionKey ?? string.Empty
                    });

                return Json(new GameCompletionResultViewModel
                {
                    GameKey = MinigameConstants.TriviaGameKey,
                    AwardedPoints = result.AwardedPoints,
                    CurrentPoints = result.CurrentPoints,
                    DailyPointsEarned = result.DailyPointsEarned,
                    DailyPointsLimit = result.DailyPointsLimit,
                    HasReachedDailyLimit = result.HasReachedDailyLimit,
                    WasCorrect = result.WasCorrect,
                    CorrectAnswers = result.CorrectAnswers,
                    CorrectAnswersToWin = result.CorrectAnswersToWin,
                    IsRoundComplete = result.IsRoundComplete,
                    NextQuestion = result.NextQuestion == null
                        ? null
                        : _minigameViewModelFactory.CreateTriviaQuestionViewModel(result.NextQuestion)
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
