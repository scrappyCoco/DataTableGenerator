using JetBrains.Annotations;

namespace Coding4fun.DataTableGenerator.Benchmark
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class CaseStat
    {
        public string iso_code { get; set; }
        public string continent { get; set; }
        public string location { get; set; }
        public string date { get; set; }
        public string total_cases { get; set; }
        public string new_cases { get; set; }
        public string new_cases_smoothed { get; set; }
        public string total_deaths { get; set; }
        public string new_deaths { get; set; }
        public string new_deaths_smoothed { get; set; }
        public string total_cases_per_million { get; set; }
        public string new_cases_per_million { get; set; }
        public string new_cases_smoothed_per_million { get; set; }
        public string total_deaths_per_million { get; set; }
        public string new_deaths_per_million { get; set; }
        public string new_deaths_smoothed_per_million { get; set; }
        public string reproduction_rate { get; set; }
        public string icu_patients { get; set; }
        public string icu_patients_per_million { get; set; }
        public string hosp_patients { get; set; }
        public string hosp_patients_per_million { get; set; }
        public string weekly_icu_admissions { get; set; }
        public string weekly_icu_admissions_per_million { get; set; }
        public string weekly_hosp_admissions { get; set; }
        public string weekly_hosp_admissions_per_million { get; set; }
        public string new_tests { get; set; }
        public string total_tests { get; set; }
        public string total_tests_per_thousand { get; set; }
        public string new_tests_per_thousand { get; set; }
        public string new_tests_smoothed { get; set; }
        public string new_tests_smoothed_per_thousand { get; set; }
        public string positive_rate { get; set; }
        public string tests_per_case { get; set; }
        public string tests_units { get; set; }
        public string total_vaccinations { get; set; }
        public string people_vaccinated { get; set; }
        public string people_fully_vaccinated { get; set; }
        public string total_boosters { get; set; }
        public string new_vaccinations { get; set; }
        public string new_vaccinations_smoothed { get; set; }
        public string total_vaccinations_per_hundred { get; set; }
        public string people_vaccinated_per_hundred { get; set; }
        public string people_fully_vaccinated_per_hundred { get; set; }
        public string total_boosters_per_hundred { get; set; }
        public string new_vaccinations_smoothed_per_million { get; set; }
        public string stringency_index { get; set; }
        public string population { get; set; }
        public string population_density { get; set; }
        public string median_age { get; set; }
        public string aged_65_older { get; set; }
        public string aged_70_older { get; set; }
        public string gdp_per_capita { get; set; }
        public string extreme_poverty { get; set; }
        public string cardiovasc_death_rate { get; set; }
        public string diabetes_prevalence { get; set; }
        public string female_smokers { get; set; }
        public string male_smokers { get; set; }
        public string handwashing_facilities { get; set; }
        public string hospital_beds_per_thousand { get; set; }
        public string life_expectancy { get; set; }
        public string human_development_index { get; set; }
        public string excess_mortality { get; set; }
    }
}