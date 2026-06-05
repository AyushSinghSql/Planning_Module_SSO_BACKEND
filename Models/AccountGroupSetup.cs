using System;
using System.Text.Json.Serialization;

public class AccountGroupSetup
{
    public string AcctGroupCode { get; set; }

    public string AccountId { get; set; }

    public string? AccountFunctionDescription { get; set; }

    public string ModifiedBy { get; set; }

    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

    public string CompanyId { get; set; }

    public string? ProjectAccountAbbreviation { get; set; }

    public bool ActiveFlag { get; set; } = true;

    public string? RevenueMappedAccount { get; set; }

    public string? SalaryCapMappedAccount { get; set; }

    [JsonIgnore]
    public virtual Account? Account { get; set; } 
}


public class AccountGroupSetupDTO
{

    public string AccountId { get; set; }

    public string? AccountFunctionDescription { get; set; }
    public string? AcctName { get; set; }
    public string? BudgetSheet { get; set; }


}
