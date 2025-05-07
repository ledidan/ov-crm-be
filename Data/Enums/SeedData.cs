
using Data.DTOs;
using Data.Entities;

namespace Data.Enums
{
    public static class SeedData
    {

        public static readonly List<Application> Applications = new()
    {
        new Application
        {
            ApplicationId = 1,
            Name = "CRM",
            Description = "Customer Relationship Management",
        },
        new Application
        {
            ApplicationId = 2,
            Name = "HRM",
            Description = "Human Resources Management",
        }
    };
        public static readonly List<CRMRole> Roles = new()
    {
        new CRMRole { Id = 1, Name = "Khách hàng - Default", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now },
        new CRMRole { Id = 2, Name = "Quản trị ứng dụng - Default", Description = "Vai trò này sẽ có đầy đủ tài các quyền. Những người dùng vai trò quản trị ứng dụng", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now },
        new CRMRole { Id = 3, Name = "Nhân viên kinh doanh - Default",Description = "Vai trò này sẽ có tất cả các quyền thuộc các quyển đã được liệt kê quan trọng", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now }
    };

        public static readonly List<CRMPermission> Permissions = new()
    {
    // Contact
    new CRMPermission { Id = 1, Action = "View", Resource = "Contact" },
    new CRMPermission { Id = 2, Action = "Create", Resource = "Contact" },
    new CRMPermission { Id = 3, Action = "Edit", Resource = "Contact" },
    new CRMPermission { Id = 4, Action = "Delete", Resource = "Contact" },

    // Customer
    new CRMPermission { Id = 5, Action = "View", Resource = "Customer" },
    new CRMPermission { Id = 6, Action = "Create", Resource = "Customer" },
    new CRMPermission { Id = 7, Action = "Edit", Resource = "Customer" },
    new CRMPermission { Id = 8, Action = "Delete", Resource = "Customer" },

    // Order
    new CRMPermission { Id = 9, Action = "View", Resource = "Order" },
    new CRMPermission { Id = 10, Action = "Create", Resource = "Order" },
    new CRMPermission { Id = 11, Action = "Edit", Resource = "Order" },
    new CRMPermission { Id = 12, Action = "Delete", Resource = "Order" },

    // Activity
    new CRMPermission { Id = 13, Action = "View", Resource = "Activity" },
    new CRMPermission { Id = 14, Action = "Create", Resource = "Activity" },
    new CRMPermission { Id = 15, Action = "Edit", Resource = "Activity" },
    new CRMPermission { Id = 16, Action = "Delete", Resource = "Activity" },

    // Invoice
    new CRMPermission { Id = 17, Action = "View", Resource = "Invoice" },
    new CRMPermission { Id = 18, Action = "Create", Resource = "Invoice" },
    new CRMPermission { Id = 19, Action = "Edit", Resource = "Invoice" },
    new CRMPermission { Id = 20, Action = "Delete", Resource = "Invoice" },

    // Product
    new CRMPermission { Id = 21, Action = "View", Resource = "Product" },
    new CRMPermission { Id = 22, Action = "Create", Resource = "Product" },
    new CRMPermission { Id = 23, Action = "Edit", Resource = "Product" },
    new CRMPermission { Id = 24, Action = "Delete", Resource = "Product" },

    // ProductCategory
    new CRMPermission { Id = 25, Action = "View", Resource = "ProductCategory" },
    new CRMPermission { Id = 26, Action = "Create", Resource = "ProductCategory" },
    new CRMPermission { Id = 27, Action = "Edit", Resource = "ProductCategory" },
    new CRMPermission { Id = 28, Action = "Delete", Resource = "ProductCategory" },

    // Inventory
    new CRMPermission { Id = 29, Action = "View", Resource = "Inventory" },
    new CRMPermission { Id = 30, Action = "Create", Resource = "Inventory" },
    new CRMPermission { Id = 31, Action = "Edit", Resource = "Inventory" },
    new CRMPermission { Id = 32, Action = "Delete", Resource = "Inventory" },

    // Quote
    new CRMPermission { Id = 33, Action = "View", Resource = "Quote" },
    new CRMPermission { Id = 34, Action = "Create", Resource = "Quote" },
    new CRMPermission { Id = 35, Action = "Edit", Resource = "Quote" },
    new CRMPermission { Id = 36, Action = "Delete", Resource = "Quote" },

    // CustomerCareTicket
    new CRMPermission { Id = 37, Action = "View", Resource = "CustomerCareTicket" },
    new CRMPermission { Id = 38, Action = "Create", Resource = "CustomerCareTicket" },
    new CRMPermission { Id = 39, Action = "Edit", Resource = "CustomerCareTicket" },
    new CRMPermission { Id = 40, Action = "Delete", Resource = "CustomerCareTicket" },

    // SupportTicket
    new CRMPermission { Id = 41, Action = "View", Resource = "SupportTicket" },
    new CRMPermission { Id = 42, Action = "Create", Resource = "SupportTicket" },
    new CRMPermission { Id = 43, Action = "Edit", Resource = "SupportTicket" },
    new CRMPermission { Id = 44, Action = "Delete", Resource = "SupportTicket" },

    // Dashboard (optional — typically just View)
    new CRMPermission { Id = 45, Action = "View", Resource = "Dashboard" },

    // Opportunity
    new CRMPermission { Id = 46, Action = "View", Resource = "Opportunity" },
    new CRMPermission { Id = 47, Action = "Create", Resource = "Opportunity" },
    new CRMPermission { Id = 48, Action = "Edit", Resource = "Opportunity" },
    new CRMPermission { Id = 49, Action = "Delete", Resource = "Opportunity" }
    };

        public static readonly List<CRMRolePermission> RolePermissions =
            new List<CRMRolePermission>
            {
            // Viewer
            new CRMRolePermission { RoleId = 1, PermissionId = 1 },
            new CRMRolePermission { RoleId = 1, PermissionId = 5 },
            new CRMRolePermission { RoleId = 1, PermissionId = 9 },

                // Admin: all
            }.Concat(
                Enumerable.Range(1, 12).Select(pid =>
                    new CRMRolePermission { RoleId = 2, PermissionId = pid }
                )
            ).Concat(new[]
            {
            // Sales: View & Edit Order
            new CRMRolePermission { RoleId = 3, PermissionId = 9 },
            new CRMRolePermission { RoleId = 3, PermissionId = 11 }
            }).ToList();

    }

}


