using FurnitureEPR.Model.Orders;
using MediatR;

namespace FurnitureEPR.Application.Features.Orders.Queries;

// دریافت لیست خلاصه سفارش‌ها با جستجو، وضعیت و صفحه‌بندی برای نمایش موبایل‌محور.
public sealed record GetOrdersQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    OrderStatus? Status = null) : IRequest<PagedOrderDto>;