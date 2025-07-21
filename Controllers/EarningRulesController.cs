using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ol_api_dotnet.Data;
using ol_api_dotnet.Models;
using Microsoft.AspNetCore.Authorization;
using ol_api_dotnet.Services;

namespace ol_api_dotnet.Controllers;

[ApiController]
[Route("api/earning-rules")]
public class EarningRulesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly EarningRuleEngineService _earningRuleEngineService;

    public class CreateEarningRuleRequest
    {
        public string Name { get; set; } = "";
        public string RuleJson { get; set; } = "";
        public bool IsActive { get; set; }
    }

    public EarningRulesController(AppDbContext context, EarningRuleEngineService earningRuleEngineService)
    {
        _context = context;
        _earningRuleEngineService = earningRuleEngineService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EarningRule>>> GetRules()
    {
        Console.WriteLine("🔍 GetRules endpoint called");
        var rules = await _context.EarningRules.ToListAsync();
        Console.WriteLine($"📝 Found {rules.Count} rules");
        return rules;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EarningRule>> GetRule(int id)
    {
        var rule = await _context.EarningRules.FindAsync(id);

        if (rule == null)
        {
            return NotFound();
        }

        return rule;
    }

    [HttpPost]
    public async Task<ActionResult<EarningRule>> CreateRule(CreateEarningRuleRequest request)
    {
        if (!_earningRuleEngineService.ValidateRuleJson(request.RuleJson))
        {
            return BadRequest("Invalid rule JSON format.");
        }

        var rule = new EarningRule
        {
            Name = request.Name,
            RuleJson = request.RuleJson,
            IsActive = request.IsActive
        };

        _context.EarningRules.Add(rule);
        await _context.SaveChangesAsync();
        await _earningRuleEngineService.ReloadRulesAsync();

        return CreatedAtAction(nameof(GetRule), new { id = rule.Id }, rule);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRule(int id, CreateEarningRuleRequest request)
    {
        if (!_earningRuleEngineService.ValidateRuleJson(request.RuleJson))
        {
            return BadRequest("Invalid rule JSON format.");
        }

        var rule = await _context.EarningRules.FindAsync(id);
        if (rule == null)
        {
            return NotFound();
        }

        rule.Name = request.Name;
        rule.RuleJson = request.RuleJson;
        rule.IsActive = request.IsActive;

        try
        {
            await _context.SaveChangesAsync();
            await _earningRuleEngineService.ReloadRulesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!RuleExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    private bool RuleExists(int id)
    {
        return _context.EarningRules.Any(e => e.Id == id);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateRuleStatus(int id, [FromBody] Dictionary<string, bool> update)
    {
        if (!update.ContainsKey("isActive"))
        {
            return BadRequest("Missing isActive field");
        }

        var rule = await _context.EarningRules.FindAsync(id);
        if (rule == null)
        {
            return NotFound();
        }

        rule.IsActive = update["isActive"];
        await _context.SaveChangesAsync();
        await _earningRuleEngineService.ReloadRulesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRule(int id)
    {
        var rule = await _context.EarningRules.FindAsync(id);
        if (rule == null)
        {
            return NotFound();
        }

        _context.EarningRules.Remove(rule);
        await _context.SaveChangesAsync();
        await _earningRuleEngineService.ReloadRulesAsync();

        return NoContent();
    }
} 