using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace ManagementSystemAPI.Controllers.OData;

[ApiController]
[Route("oData/")]
public class ODataBaseController : ODataController
{
    private ISender _mediator = null!;
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetService<ISender>();
}