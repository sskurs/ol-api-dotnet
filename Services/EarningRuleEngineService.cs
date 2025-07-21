using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ol_api_dotnet.Data;
using ol_api_dotnet.Models;
using RulesEngine.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ol_api_dotnet.Services;

public class EarningRuleEngineService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly RulesEngine.RulesEngine _rulesEngine;

    public EarningRuleEngineService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _rulesEngine = new RulesEngine.RulesEngine();
        LoadActiveRules().GetAwaiter().GetResult();
    }

    private async Task LoadActiveRules()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var activeRules = await context.EarningRules.Where(r => r.IsActive).ToListAsync();

        var workflows = activeRules
            .Select(r => JsonConvert.DeserializeObject<Workflow>(r.RuleJson))
            .Where(w => w != null)
            .ToArray();

        _rulesEngine.ClearWorkflows();
        foreach (var workflow in workflows)
        {
            if (workflow != null) _rulesEngine.AddOrUpdateWorkflow(workflow);
        }
    }

    public async Task ReloadRulesAsync()
    {
        await LoadActiveRules();
    }

    public async Task<int> EvaluateTransaction(Transaction transaction)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var input = new
        {
            PurchaseAmount = transaction.Amount,
            TransactionCount = await context.Transactions
                .CountAsync(t => t.UserId == transaction.UserId),
            transaction.UserId,
            MerchantId = transaction.MerchantId ?? 0,
            TransactionDate = transaction.Date
        };

        int totalPoints = 0;
        var workflowNames = _rulesEngine.GetAllRegisteredWorkflowNames();

        foreach (var workflowName in workflowNames)
        {
            try
            {
                Console.WriteLine($"[RuleEngine] Evaluating workflow: {workflowName} with input: {System.Text.Json.JsonSerializer.Serialize(input)}");
                var result = await _rulesEngine.ExecuteAllRulesAsync(workflowName, input);
                Console.WriteLine($"[RuleEngine] Result for {workflowName}: {System.Text.Json.JsonSerializer.Serialize(result)}");
                var successResult = result.FirstOrDefault(r => r.IsSuccess);

                if (successResult?.ActionResult != null)
                {
                    totalPoints += Convert.ToInt32(successResult.ActionResult.Output);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error evaluating workflow {workflowName}: {ex.Message}");
                continue;
            }
        }

        return totalPoints;
    }

    public bool ValidateRuleJson(string ruleJson)
    {
        try
        {
            var workflow = JsonConvert.DeserializeObject<Workflow>(ruleJson);
            if (workflow == null) return false;

            return workflow.WorkflowName != null && workflow.Rules != null && workflow.Rules.Any();
        }
        catch
        {
            return false;
        }
    }
} 