using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Coding4fun.DataTableGenerator.Benchmark
{
    public class SqlMappingWithExpression
    {
        private readonly List<Func<CaseStat, object>> _cellGetters = new();
        public readonly DataTable DataTable = new();
            
        public SqlMappingWithExpression AddColumn(string columnName, string columnType, Expression<Func<CaseStat, object>> getCellExpression)
        {
            var func = getCellExpression.Compile();
            DataTable.Columns.Add(columnName, typeof(string));
            _cellGetters.Add(func);
            return this;
        }

        public void AddRows(IEnumerable<CaseStat> items)
        {
            DataTable.Clear();
            foreach (var caseStat in items)
            {
                DataTable.Rows.Add(_cellGetters.Select(fun => fun.Invoke(caseStat)).ToArray());
            }
        }

        public static SqlMappingWithExpression Create() => new SqlMappingWithExpression()
            .AddColumn("iso_code", "string", c => c.iso_code)
                .AddColumn("continent", "string", c => c.continent)
                .AddColumn("location", "string", c => c.location)
                .AddColumn("date", "string", c => c.date)
                .AddColumn("total_cases", "string", c => c.total_cases)
                .AddColumn("new_cases", "string", c => c.new_cases)
                .AddColumn("new_cases_smoothed", "string", c => c.new_cases_smoothed)
                .AddColumn("total_deaths", "string", c => c.total_deaths)
                .AddColumn("new_deaths", "string", c => c.new_deaths)
                .AddColumn("new_deaths_smoothed", "string", c => c.new_deaths_smoothed)
                .AddColumn("total_cases_per_million", "string", c => c.total_cases_per_million)
                .AddColumn("new_cases_per_million", "string", c => c.new_cases_per_million)
                .AddColumn("new_cases_smoothed_per_million", "string", c => c.new_cases_smoothed_per_million)
                .AddColumn("total_deaths_per_million", "string", c => c.total_deaths_per_million)
                .AddColumn("new_deaths_per_million", "string", c => c.new_deaths_per_million)
                .AddColumn("new_deaths_smoothed_per_million", "string", c => c.new_deaths_smoothed_per_million)
                .AddColumn("reproduction_rate", "string", c => c.reproduction_rate)
                .AddColumn("icu_patients", "string", c => c.icu_patients)
                .AddColumn("icu_patients_per_million", "string", c => c.icu_patients_per_million)
                .AddColumn("hosp_patients", "string", c => c.hosp_patients)
                .AddColumn("hosp_patients_per_million", "string", c => c.hosp_patients_per_million)
                .AddColumn("weekly_icu_admissions", "string", c => c.weekly_icu_admissions)
                .AddColumn("weekly_icu_admissions_per_million", "string", c => c.weekly_icu_admissions_per_million)
                .AddColumn("weekly_hosp_admissions", "string", c => c.weekly_hosp_admissions)
                .AddColumn("weekly_hosp_admissions_per_million", "string", c => c.weekly_hosp_admissions_per_million)
                .AddColumn("new_tests", "string", c => c.new_tests)
                .AddColumn("total_tests", "string", c => c.total_tests)
                .AddColumn("total_tests_per_thousand", "string", c => c.total_tests_per_thousand)
                .AddColumn("new_tests_per_thousand", "string", c => c.new_tests_per_thousand)
                .AddColumn("new_tests_smoothed", "string", c => c.new_tests_smoothed)
                .AddColumn("new_tests_smoothed_per_thousand", "string", c => c.new_tests_smoothed_per_thousand)
                .AddColumn("positive_rate", "string", c => c.positive_rate)
                .AddColumn("tests_per_case", "string", c => c.tests_per_case)
                .AddColumn("tests_units", "string", c => c.tests_units)
                .AddColumn("total_vaccinations", "string", c => c.total_vaccinations)
                .AddColumn("people_vaccinated", "string", c => c.people_vaccinated)
                .AddColumn("people_fully_vaccinated", "string", c => c.people_fully_vaccinated)
                .AddColumn("total_boosters", "string", c => c.total_boosters)
                .AddColumn("new_vaccinations", "string", c => c.new_vaccinations)
                .AddColumn("new_vaccinations_smoothed", "string", c => c.new_vaccinations_smoothed)
                .AddColumn("total_vaccinations_per_hundred", "string", c => c.total_vaccinations_per_hundred)
                .AddColumn("people_vaccinated_per_hundred", "string", c => c.people_vaccinated_per_hundred)
                .AddColumn("people_fully_vaccinated_per_hundred", "string", c => c.people_fully_vaccinated_per_hundred)
                .AddColumn("total_boosters_per_hundred", "string", c => c.total_boosters_per_hundred)
                .AddColumn("new_vaccinations_smoothed_per_million", "string",
                    c => c.new_vaccinations_smoothed_per_million)
                .AddColumn("stringency_index", "string", c => c.stringency_index)
                .AddColumn("population", "string", c => c.population)
                .AddColumn("population_density", "string", c => c.population_density)
                .AddColumn("median_age", "string", c => c.median_age)
                .AddColumn("aged_65_older", "string", c => c.aged_65_older)
                .AddColumn("aged_70_older", "string", c => c.aged_70_older)
                .AddColumn("gdp_per_capita", "string", c => c.gdp_per_capita)
                .AddColumn("extreme_poverty", "string", c => c.extreme_poverty)
                .AddColumn("cardiovasc_death_rate", "string", c => c.cardiovasc_death_rate)
                .AddColumn("diabetes_prevalence", "string", c => c.diabetes_prevalence)
                .AddColumn("female_smokers", "string", c => c.female_smokers)
                .AddColumn("male_smokers", "string", c => c.male_smokers)
                .AddColumn("handwashing_facilities", "string", c => c.handwashing_facilities)
                .AddColumn("hospital_beds_per_thousand", "string", c => c.hospital_beds_per_thousand)
                .AddColumn("life_expectancy", "string", c => c.life_expectancy)
                .AddColumn("human_development_index", "string", c => c.human_development_index)
                .AddColumn("excess_mortality", "string", c => c.excess_mortality);
    }
}