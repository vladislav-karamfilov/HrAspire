﻿namespace HrAspire.Web.Common.Models.Employees;

using System;

public record EmployeeResponseModel(
    string Id,
    string Email,
    string FullName,
    DateOnly DateOfBirth,
    string? Department,
    string Position,
    DateTime CreatedOn);