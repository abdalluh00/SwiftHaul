using Microsoft.AspNetCore.Mvc;
using SharedKernel.Common;

namespace SharedKernel.Extensions
{
    public static class ControllerExtensions
    {
        public static IActionResult ToActionResult<T>(
            this ControllerBase controller,
            Result<T> result)
        {
            if (result.IsSuccess)
                return controller.Ok(result.Value);

            return result.Error.Code switch
            {
                "NOT_FOUND" => controller.NotFound(new { error = result.Error.Message }),
                "VALIDATION_ERROR" => controller.BadRequest(new { error = result.Error.Message }),
                "UNAUTHORIZED" => controller.Unauthorized(new { error = result.Error.Message }),
                "CONFLICT" => controller.Conflict(new { error = result.Error.Message }),
                _ => controller.StatusCode(500, new { error = result.Error.Message })
            };
        }

        public static IActionResult ToActionResult(
            this ControllerBase controller,
            Result result)
        {
            if (result.IsSuccess)
                return controller.NoContent();

            return result.Error.Code switch
            {
                "NOT_FOUND" => controller.NotFound(new { error = result.Error.Message }),
                "VALIDATION_ERROR" => controller.BadRequest(new { error = result.Error.Message }),
                "UNAUTHORIZED" => controller.Unauthorized(new { error = result.Error.Message }),
                "CONFLICT" => controller.Conflict(new { error = result.Error.Message }),
                _ => controller.StatusCode(500, new { error = result.Error.Message })
            };
        }
    }
}
