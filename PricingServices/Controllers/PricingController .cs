using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PricingServices.Common.Helpers;
using PricingServices.Domain.Entities;
using PricingServices.Infrastructure.Data;
using static PricingServices.Application.DTOs.PricingDTOs;

namespace PricingServices.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PricingController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly PricingHelper _pricingHelper;

        public PricingController(AppDbContext context, PricingHelper pricingHelper)
        {
            _context = context;
            _pricingHelper = pricingHelper;
        }

        // Anyone can estimate price before creating shipment
        [HttpPost("estimate")]
        [AllowAnonymous]
        public IActionResult EstimatePrice(PriceEstimateRequest request)
        {
            var (total, multiplier) = _pricingHelper.Calculate(
                request.PickupCity,
                request.DeliveryCity,
                request.WeightKg);

            return Ok(new PriceEstimateResponse(
                request.PickupCity,
                request.DeliveryCity,
                request.WeightKg,
                _pricingHelper.GetBasePricePerKg(),
                multiplier,
                total
            ));
        }



        // Called internally when shipment is created
        [HttpPost("invoice")]
        [Authorize(Roles = "Customer,Company,Admin")]
        public async Task<IActionResult> CreateInvoice(CreateInvoiceRequest request)
        {
            // Check invoice already exists for this shipment
            var exists = await _context.Invoices
                .AnyAsync(i => i.ShipmentId == request.ShipmentId);

            if (exists)
                return BadRequest("Invoice already exists for this shipment.");

            var (total, multiplier) = _pricingHelper.Calculate(
                request.PickupCity,
                request.DeliveryCity,
                request.WeightKg);

            var invoice = new Invoice
            {
                InvoiceNumber = InvoiceNumberHelper.Generate(),
                ShipmentId = request.ShipmentId,
                CustomerId = request.CustomerId,
                PickupCity = request.PickupCity,
                DeliveryCity = request.DeliveryCity,
                WeightKg = request.WeightKg,
                BasePricePerKg = _pricingHelper.GetBasePricePerKg(),
                DistanceMultiplier = multiplier,
                TotalAmount = total
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return Ok(ToResponse(invoice));
        }


        // Mark invoice as paid
        [HttpPut("{id}/pay")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkAsPaid(Guid id)
        {
            var invoice = await _context.Invoices.FindAsync(id);

            if (invoice is null)
                return NotFound("Invoice not found.");

            if (invoice.Status == InvoiceStatus.Paid)
                return BadRequest("Invoice is already paid.");

            invoice.Status = InvoiceStatus.Paid;
            invoice.PaidAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("Invoice marked as paid.");
        }



        private static InvoiceResponse ToResponse(Invoice invoice) => new(
       invoice.Id,
       invoice.InvoiceNumber,
       invoice.ShipmentId,
       invoice.PickupCity,
       invoice.DeliveryCity,
       invoice.WeightKg,
       invoice.BasePricePerKg,
       invoice.DistanceMultiplier,
       invoice.TotalAmount,
       invoice.Status.ToString(),
       invoice.CreatedAt
   );
    }
}