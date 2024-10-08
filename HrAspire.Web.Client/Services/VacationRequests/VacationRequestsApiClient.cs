﻿namespace HrAspire.Web.Client.Services.VacationRequests;

using System.Net.Http.Json;

using HrAspire.Web.Common.Models.SalaryRequests;
using HrAspire.Web.Common.Models.VacationRequests;

public class VacationRequestsApiClient
{
    private readonly HttpClient httpClient;

    public VacationRequestsApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public Task<SalaryRequestsResponseModel> GetSalaryRequestsAsync(int pageNumber, int pageSize)
        => this.httpClient.GetFromJsonAsync<SalaryRequestsResponseModel>($"salaryRequests?pageNumber={pageNumber}&pageSize={pageSize}")!;

    public Task<VacationRequestsResponseModel> GetEmployeeVacationRequestsAsync(string employeeId, int pageNumber, int pageSize)
        => this.httpClient.GetFromJsonAsync<VacationRequestsResponseModel>(
            $"employees/{employeeId}/vacationRequests?pageNumber={pageNumber}&pageSize={pageSize}")!;

    public async Task<(VacationRequestDetailsResponseModel? VacationRequest, string? ErrorMessage)> GetVacationRequestAsync(int id)
    {
        var response = await this.httpClient.GetAsync($"vacationRequests/{id}");
        if (response.IsSuccessStatusCode)
        {
            var vacationRequest = await response.Content.ReadFromJsonAsync<VacationRequestDetailsResponseModel>();
            return (vacationRequest, null);
        }

        var errorMessage = await response.GetErrorMessageAsync();
        return (null, errorMessage);
    }

    public async Task<(int? NewVacationRequestId, string? ErrorMessage)> CreateVacationRequestAsync(
        VacationRequestCreateRequestModel request)
    {
        var response = await this.httpClient.PostAsJsonAsync("vacationRequests", request);
        if (response.IsSuccessStatusCode)
        {
            var vacationRequestId = await response.Content.ReadFromJsonAsync<int>();
            return (vacationRequestId, null);
        }

        var errorMessage = await response.GetErrorMessageAsync();
        return (null, errorMessage);
    }

    public async Task<string?> UpdateVacationRequestAsync(int id, VacationRequestUpdateRequestModel request)
    {
        var response = await this.httpClient.PutAsJsonAsync($"vacationRequests/{id}", request);
        if (response.IsSuccessStatusCode)
        {
            return null;
        }

        var errorMessage = await response.GetErrorMessageAsync();
        return errorMessage;
    }

    public async Task<string?> DeleteVacationRequestAsync(int id)
    {
        var response = await this.httpClient.DeleteAsync($"vacationRequests/{id}");
        if (response.IsSuccessStatusCode)
        {
            return null;
        }

        var errorMessage = await response.GetErrorMessageAsync();
        return errorMessage;
    }

    public async Task<string?> ApproveVacationRequestAsync(int id)
    {
        var response = await this.httpClient.PostAsync($"vacationRequests/{id}/approval", content: null);
        if (response.IsSuccessStatusCode)
        {
            return null;
        }

        var errorMessage = await response.GetErrorMessageAsync();
        return errorMessage;
    }

    public async Task<string?> RejectVacationRequestAsync(int id)
    {
        var response = await this.httpClient.PostAsync($"vacationRequests/{id}/rejection", content: null);
        if (response.IsSuccessStatusCode)
        {
            return null;
        }

        var errorMessage = await response.GetErrorMessageAsync();
        return errorMessage;
    }
}
