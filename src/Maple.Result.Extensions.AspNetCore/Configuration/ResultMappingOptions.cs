using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Maple.Result.Extensions.AspNetCore.Configuration;

/// <summary>
///     Represents the options for configuring result mappings.
/// </summary>
public class ResultMappingOptions
{
    /// <summary>
    ///     Gets the list of custom mappings for mapping errors to <see cref="IActionResult" />.
    /// </summary>
    public List<Func<Error, ControllerBase, IActionResult?>> ErrorMappings { get; } = [];
}
