using System.Security.Claims;
using Data.DTOs;
using Data.Entities;
using Data.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Server.Controllers;
using ServerLibrary.Services.Interfaces;

public class PartnerControllerTests
{
    private readonly Mock<ICRMService> _crmServiceMock;
    private readonly Mock<IPartnerService> _partnerServiceMock;
    private readonly Mock<IUserService> _userServiceMock;

    private readonly Mock<IEmployeeService> _employeeServiceMock;
    private readonly PartnerController _controller;

    public PartnerControllerTests()
    {
        _crmServiceMock = new Mock<ICRMService>();
        _partnerServiceMock = new Mock<IPartnerService>();
        _userServiceMock = new Mock<IUserService>();
        _employeeServiceMock = new Mock<IEmployeeService>();
        _controller = new PartnerController(
            _partnerServiceMock.Object,
            _userServiceMock.Object,
            _crmServiceMock.Object,
            _employeeServiceMock.Object
        );
    }

    private ClaimsIdentity CreateMockIdentity(string isOwner = "true")
    {
        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim("Owner", isOwner));
        return identity;
    }

    // [Fact]
    // public async Task InitializePartnerAsync_ReturnsOk_WhenOwnerAndValid()
    // {
    //     var mockIdentity = CreateMockIdentity();
    //     var mockHttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(mockIdentity) };
    //     _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

    //     var request = new RequestInitializePartner { UserId = 10 };

    //     _partnerServiceMock.Setup(x => x.FindByClaim(mockIdentity))
    //         .ReturnsAsync(new Partner { Id = 1 });

    //     _partnerServiceMock.Setup(x => x.CheckClaimByOwner(mockIdentity))
    //         .ReturnsAsync(true);

    //     _crmServiceMock.Setup(x => x.FirstSetupCRMPartnerAsync(1, 10, 2))
    //         .ReturnsAsync(new DataObjectResponse(true, "OK"));

    //     var result = await _controller.InitializePartnerAsync(request);

    //     var okResult = Assert.IsType<OkObjectResult>(result);
    //     var response = Assert.IsType<DataObjectResponse>(okResult.Value);
    //     Assert.True(response.Flag);
    // }

    // [Fact]
    // public async Task InitializePartnerAsync_ReturnsForbid_WhenNotOwner()
    // {
    //     var mockIdentity = CreateMockIdentity("false");
    //     var mockHttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(mockIdentity) };
    //     _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

    //     var request = new RequestInitializePartner { UserId = 10 };

    //     _partnerServiceMock.Setup(x => x.FindByClaim(mockIdentity))
    //         .ReturnsAsync(new Partner { Id = 1 });

    //     _partnerServiceMock.Setup(x => x.CheckClaimByOwner(mockIdentity))
    //         .ReturnsAsync(false);

    //     var result = await _controller.InitializePartnerAsync(request);

    //     Assert.IsType<ForbidResult>(result);
    // }

    // [Fact]
    // public async Task InitializePartnerAsync_ReturnsNotFound_WhenPartnerOrUserInvalid()
    // {
    //     var mockIdentity = CreateMockIdentity();
    //     var mockHttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(mockIdentity) };
    //     _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

    //     var request = new RequestInitializePartner { UserId = 10 };

    //     _partnerServiceMock.Setup(x => x.FindByClaim(mockIdentity))
    //         .ReturnsAsync(new Partner { Id = -1 });

    //     var result = await _controller.InitializePartnerAsync(request);

    //     var notFound = Assert.IsType<NotFoundObjectResult>(result);
    //     Assert.Equal("Partner not found or user not found", notFound.Value);
    // }
}

