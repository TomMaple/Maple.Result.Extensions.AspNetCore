# Maple.Result.Extensions.Functions.Worker.AspNetCore
Maps the [Maple.Result](https://github.com/TomMaple/Result) to the HTTP response in Azure Functions (isolated worker
model) with ASP.NET Core integration.

The functions using `HttpRequestData` and `HttpResponseData`, without ASP.NET Core integration, should use
[Maple.Result.Extensions.Functions.Worker](https://www.nuget.org/packages/Maple.Result.Extensions.Functions.Worker/).

# Project status: EARLY STAGE

# Usage
```csharp
using Maple.Result;
using Maple.Result.Extensions.Functions.Worker.AspNetCore;

// Success -> 204 No Content, error -> RFC 9457 problem details
[Function("DeleteOrder")]
public IActionResult DeleteOrder(
    [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "orders/{id:int}")] HttpRequest request, int id)
    => _orderService.Delete(id).ToActionResult();

// Success -> 200 OK with the value (204 No Content when the value is null)
[Function("GetOrder")]
public IActionResult GetOrder(
    [HttpTrigger(AuthorizationLevel.Function, "get", Route = "orders/{id:int}")] HttpRequest request, int id)
    => _orderService.Get(id).ToActionResult();

// Success -> the given status code
[Function("CreateOrder")]
public IActionResult CreateOrder(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = "orders")] HttpRequest request)
    => _orderService.Create().ToActionResult(HttpStatusCode.Created);

// Success -> mapped explicitly, here to 201 Created with the Location header
[Function("CopyOrder")]
public IActionResult CopyOrder(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = "orders/{id:int}/copy")] HttpRequest request, int id)
    => _orderService.Copy(id).ToActionResult((Order order) => new CreatedResult($"/orders/{order.Id}", order));

// Errors -> mapped explicitly; the default mapping is used when the mapping returns null
[Function("DeleteOrderLines")]
public IActionResult DeleteOrderLines(
    [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "orders/{id:int}/lines")] HttpRequest request, int id)
    => _orderService.DeleteLines(id).ToActionResult(error => error.Category == ErrorCategory.Conflict
        ? new StatusCodeResult(StatusCodes.Status410Gone)
        : null);
```

# Error responses
Errors are returned as RFC 9457 problem details, in the same format as in
[Maple.Result.Extensions.AspNetCore](https://www.nuget.org/packages/Maple.Result.Extensions.AspNetCore/). They include
the `traceId` extension, and the `CustomizeProblemDetails` option of `AddProblemDetails` is applied to them.

# Good to know
* There are no global mappings. To use the same mapping in many functions, pass a shared static method.
* The mapping functions do not receive any context. If they need the request or the response, they can capture
  the `HttpRequest` parameter of the function.
* On `Result<T>`, a lambda mapping a successful result needs an explicitly typed parameter, e.g.,
  `(Order order) => ...`. Otherwise, the overload taking the error mapping is selected.

# Contribution
Please contact author: engineer(at sign)blumail(dot)me
