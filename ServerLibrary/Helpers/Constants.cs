using Data.Entities;

namespace ServerLibrary.Helpers
{
    public static class Constants
    {
        public static class Role
        {
            public static string SysAdmin { get; } = "SysAdmin";
            public static string Admin { get; } = "Admin";
            public static string User { get; } = "User";
        }
        
        public static class EmployeeId {
            public static int SuperAdmin { get; } = 99999;
        }
        public static class DefaultJobPositionGroups
        {
            public static readonly List<JobPositionGroup> JobPositionGroups = new()
        {
            new JobPositionGroup { Id = 1, JobPositionGroupCode = Guid.NewGuid().ToString(), JobPositionGroupName = "Hành chính",  },
            new JobPositionGroup { Id = 2, JobPositionGroupCode = Guid.NewGuid().ToString(), JobPositionGroupName = "Nhân sự",  },
            new JobPositionGroup { Id = 3, JobPositionGroupCode = Guid.NewGuid().ToString(), JobPositionGroupName = "Kế toán",},
            new JobPositionGroup { Id = 4, JobPositionGroupCode = Guid.NewGuid().ToString(), JobPositionGroupName = "Kinh doanh",  },
            new JobPositionGroup { Id = 5, JobPositionGroupCode = Guid.NewGuid().ToString(), JobPositionGroupName = "Kỹ thuật",  },
            new JobPositionGroup { Id = 6, JobPositionGroupCode = Guid.NewGuid().ToString(), JobPositionGroupName = "Công nhân",  },
            new JobPositionGroup { Id = 7, JobPositionGroupCode = Guid.NewGuid().ToString(), JobPositionGroupName = "Quản lý",  },
            new JobPositionGroup { Id = 8, JobPositionGroupCode = Guid.NewGuid().ToString(), JobPositionGroupName = "Lãnh đạo",  }
        };
        }
        public static class DefaultJobTitleGroups
        {
            public static readonly List<JobTitleGroup> JobTitleGroups = new()
        {
            new JobTitleGroup { Id = 1, JobTitleGroupCode = Guid.NewGuid().ToString(), JobTitleGroupName = "Nhân viên", },
            new JobTitleGroup { Id = 2, JobTitleGroupCode = Guid.NewGuid().ToString(), JobTitleGroupName = "Phó phòng", },
            new JobTitleGroup { Id = 3, JobTitleGroupCode = Guid.NewGuid().ToString(), JobTitleGroupName = "Trưởng phòng",  },
            new JobTitleGroup { Id = 4, JobTitleGroupCode = Guid.NewGuid().ToString(), JobTitleGroupName = "Phó giám đốc",  },
            new JobTitleGroup { Id = 5, JobTitleGroupCode = Guid.NewGuid().ToString(), JobTitleGroupName = "Giám đốc",  },
            new JobTitleGroup { Id = 6, JobTitleGroupCode = Guid.NewGuid().ToString(), JobTitleGroupName = "Phó tổng giám đốc",  },
            new JobTitleGroup { Id = 7, JobTitleGroupCode = Guid.NewGuid().ToString(), JobTitleGroupName = "Tổng giám đốc",  },
            new JobTitleGroup { Id = 8, JobTitleGroupCode = Guid.NewGuid().ToString(), JobTitleGroupName = "Chủ tịch HĐQT",  }
        };
        }

    }
}
