
using System.ComponentModel.DataAnnotations;

namespace Data.DTOs
{
    public class CreateMissionDTO
    {
        public string? MissionName { get; set; }
        public string? MissionTypeID { get; set; }

    }
}