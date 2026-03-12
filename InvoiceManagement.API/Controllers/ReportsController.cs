using InvoiceManagement.BLL.Models;
using InvoiceManagement.BLL.Services.Interfaces;
using InvoiceManagement.DAL.Documents;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceManagement.API.Controllers
{
    /// <summary>
    /// Financial reports, analytics snapshots, and audit trails
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportingService _svc;
        public ReportsController(IReportingService svc) => _svc = svc;

        // GET api/reports/aging
        [HttpGet("aging")]
        [ProducesResponseType(typeof(AgingReportDto), 200)]
        public async Task<IActionResult> GetAgingReport()
        {
            var report = await _svc.GetAgingReportAsync();
            return Ok(report);
        }

        // GET api/reports/dso?periodDays=30
        [HttpGet("dso")]
        [ProducesResponseType(typeof(DsoReportDto), 200)]
        public async Task<IActionResult> GetDso([FromQuery] int periodDays = 30)
        {
            if (periodDays <= 0) return BadRequest(new { message = "periodDays must be > 0" });
            var dso = await _svc.GetDsoAsync(periodDays);
            return Ok(dso);
        }

        // POST api/reports/snapshot?year=2024&month=3
        [HttpPost("snapshot")]
        [ProducesResponseType(typeof(AnalyticsSnapshotDocument), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GenerateSnapshot(
            [FromQuery] int? year,
            [FromQuery] int? month)
        {
            var y = year  ?? DateTime.UtcNow.Year;
            var m = month ?? DateTime.UtcNow.Month;
            if (m < 1 || m > 12) return BadRequest(new { message = "Month must be 1–12." });

            var snapshot = await _svc.GenerateMonthlySnapshotAsync(y, m);
            return Ok(snapshot);
        }

        // GET api/reports/snapshots?year=2024
        [HttpGet("snapshots")]
        [ProducesResponseType(typeof(IEnumerable<AnalyticsSnapshotDocument>), 200)]
        public async Task<IActionResult> GetSnapshots([FromQuery] int? year)
        {
            var y = year ?? DateTime.UtcNow.Year;
            var snapshots = await _svc.GetSnapshotsAsync(y);
            return Ok(snapshots);
        }

        // GET api/reports/audit/{invoiceId}
        [HttpGet("audit/{invoiceId}")]
        [ProducesResponseType(typeof(IEnumerable<AuditLogDocument>), 200)]
        public async Task<IActionResult> GetAuditTrail(string invoiceId)
        {
            var logs = await _svc.GetAuditTrailAsync(invoiceId);
            return Ok(logs);
        }

        // GET api/reports/reconciliation/{invoiceId}
        [HttpGet("reconciliation/{invoiceId}")]
        [ProducesResponseType(typeof(IEnumerable<ReconciliationDocument>), 200)]
        public async Task<IActionResult> GetReconciliation(string invoiceId)
        {
            var records = await _svc.GetReconciliationsAsync(invoiceId);
            return Ok(records);
        }
    }
}
