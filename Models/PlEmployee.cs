using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PlanningAPI.Models;

public partial class PlEmployee
{
    public string EmplId { get; set; } = null!;

    public string? OrgId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? MidName { get; set; }

    public string? Role { get; set; }

    public string? Email { get; set; } = null!;

    public string? Ln1Adr { get; set; }

    public string? Ln2Adr { get; set; }

    public string? Ln3Adr { get; set; }

    public string? MailStateDc { get; set; }

    public string? PostalCd { get; set; }

    public string? CountyName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? MaritalCd { get; set; }

    public string? Gender { get; set; }

    public DateOnly? HireDate { get; set; }

    public string? PlcGlcCode { get; set; }

    public decimal? PerHourRate { get; set; }

    public decimal? Salary { get; set; }

    public string? AccId { get; set; }
    public string? Type { get; set; }

    public bool IsRev { get; set; }

    public bool IsBrd { get; set; }

    public DateTime? CreatedAt { get; set; }
    [JsonIgnore]
    public virtual Organization? Org { get; set; }
    
    public virtual ICollection<PlForecast> PlForecasts { get; set; } = new List<PlForecast>();
}

public class EmployeeMaster
{
    public string EmplId { get; set; } = null!; // Required

    public string? LvPdCd { get; set; }
    public string? TaxbleEntityId { get; set; }
    public string? SsnId { get; set; }
    public DateTime? OrigHireDt { get; set; }
    public DateTime? AdjHireDt { get; set; }
    public DateTime? TermDt { get; set; }
    public string? SEmplStatusCd { get; set; }
    public string? SpvsrName { get; set; }
    public string? LastName { get; set; }
    public string? FirstName { get; set; }
    public string? MidName { get; set; }
    public string? PrefName { get; set; }
    public string? NamePrfxCd { get; set; }
    public string? NameSfxCd { get; set; }
    public string? Notes { get; set; }
    public string? TsPdCd { get; set; }
    public DateTime? BirthDt { get; set; }
    public string? CityName { get; set; }
    public string? CountryCd { get; set; }
    public string? LastFirstName { get; set; }
    public string? Ln1Adr { get; set; }
    public string? Ln2Adr { get; set; }
    public string? Ln3Adr { get; set; }
    public string? MailStateDc { get; set; }
    public string? PostalCd { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? TimeStamp { get; set; }
    public string? LocatorCd { get; set; }
    public string? PrirName { get; set; }
    public string? CompanyId { get; set; }
    public DateTime? LastReviewDt { get; set; }
    public DateTime? NextReviewDt { get; set; }
    public string? SexCd { get; set; }
    public string? MaritalCd { get; set; }
    public bool? EligAutoPayFl { get; set; }
    public string? EmailId { get; set; }
    public string? MgrEmplId { get; set; }
    public string? SRaceCd { get; set; }
    public string? PrServEmplId { get; set; }
    public string? CountyName { get; set; }
    public decimal? TsPdRegHrsNo { get; set; }
    public decimal? PayPdRegHrsNo { get; set; }
    public bool? DisabledFl { get; set; }
    public int? MosReviewNo { get; set; }
    public string? ContName1 { get; set; }
    public string? ContName2 { get; set; }
    public string? ContPhone1 { get; set; }
    public string? ContPhone2 { get; set; }
    public string? ContRel1 { get; set; }
    public string? ContRel2 { get; set; }
    public bool? UnionEmplFl { get; set; }
    public string? VisaTypeCd { get; set; }
    public string? VetStatusS { get; set; }
    public string? VetStatusV { get; set; }
    public string? VetStatusO { get; set; }
    public string? VetStatusR { get; set; }
    public string? EssPinId { get; set; }
    public bool? PinUpdatedFl { get; set; }
    public string? SEssCosCd { get; set; }
    public string? HomeEmailId { get; set; }
    public byte[]? Rowversion { get; set; }
    public DateTime? VetReleaseDt { get; set; }
    public bool? ContractorFl { get; set; }
    public bool? BlindFl { get; set; }
    public DateTime? VisaDt { get; set; }
    public string? VetStatusD { get; set; }
    public string? VetStatusA { get; set; }
    public string? TimeEntryType { get; set; }
    public string? BadgeGroup { get; set; }
    public string? BadgeId { get; set; }
    public string? LoginId { get; set; }
    public bool? SftFl { get; set; }
    public bool? MesFl { get; set; }
    public bool? ClockFl { get; set; }
    public string? PlantId { get; set; }
    public string? EmplSourceCd { get; set; }
    public DateTime? SrExportDt { get; set; }
    public DateTime? HrsmartExportDt { get; set; }
    public string? VetStatusP { get; set; }
    public string? BirthCityName { get; set; }
    public string? BirthMailStateDc { get; set; }
    public string? BirthCountryCd { get; set; }
    public string? UserLoginId { get; set; }
    public string? EmplAuthMthd { get; set; }
    public bool? EssUserFl { get; set; }
    public DateTime? LastDayDt { get; set; }
    public string? GovwiniqLoginId { get; set; }
    public string? HuaId { get; set; }
    public bool? HuaActvMapFl { get; set; }
    public string? VetStatusNp { get; set; }
    public string? VetStatusDeclined { get; set; }
    public string? VetStatusRs { get; set; }
}

public class PlEmployeee
{
    public int Id { get; set; }

    public string? EmplId { get; set; } = null!;
    public string? OrgId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public string? PlcGlcCode { get; set; }
    public decimal? PerHourRate { get; set; }
    public decimal? Salary { get; set; }
    public decimal? Esc_Percent { get; set; }
    public string? AccId { get; set; }
    public DateOnly? HireDate { get; set; }
    public bool? IsRev { get; set; }
    public bool? IsBrd { get; set; }
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Type { get; set; }
    public string? Status { get; set; }

    public int? PlId { get; set; }
    [NotMapped]
    public bool isWarning { get; set; } = false;
    public DateOnly? EffectiveDate { get; set; }
    public virtual ICollection<PlForecast>? PlForecasts { get; set; } = new List<PlForecast>();
    // Navigation properties
    public virtual Organization? Organization { get; set; }
    public virtual PlProjectPlan? PlProjectPlan { get; set; }
    //public DateOnly? Esc_Month { get; internal set; }
}


public class Empl_Master
{
    public string EmplId { get; set; } = null!;

    public string? Status { get; set; }

    public string? FirstName { get; set; }
    public decimal? PerHourRate { get; set; }
    public decimal? Salary { get; set; }
    public DateOnly? EffectiveDate { get; set; }

}
public class Empl_Master_Dto
{
    public string EmplId { get; set; } = null!;

    public string? Status { get; set; }

    public string? FirstName { get; set; }
    public decimal? PerHourRate { get; set; }
    public decimal? Salary { get; set; }
    public DateOnly? EffectiveDate { get; set; }
    public string? Genl_Lab_Cat_CD { get; set; }
    public string? Bill_Lab_Cat_CD { get; set; }

}

public class EmployeeDto
{
    public int EmplId { get; set; }
    public DateTime? Orig_Hire_Dt { get; set; }
    public DateTime? Term_Dt { get; set; }
    public string Last_Name { get; set; }
    public string First_Name { get; set; }
    public string Last_First_Name { get; set; }
}



