using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services
{
    public class GetCountVehicleOuput
    {
        public int ActiveCarNumber {  get; set; }
        public int ActiveMotorNumber { get; set; }
        public int ActiveBikeNumber { get; set; }
        public int ActiveOtherNumber { get; set; }
        public int ActiveECarNumber { get; set; }
        public int ActiveEMotorNumber { get; set; }
        public int ActiveEBikeNumber { get; set; }

        public int WaitingCarNumber { get; set; }
        public int WaitingMotorNumber { get; set; }
        public int WaitingBikeNumber { get; set; }
        public int WaitingOtherNumber { get; set; }
        public int WaitingECarNumber { get; set; }
        public int WaitingEMotorNumber { get; set; }
        public int WaitingEBikeNumber { get; set; }

        public int InactiveCarNumber { get; set; }
        public int InactiveMotorNumber { get; set; }
        public int InactiveBikeNumber { get; set; }
        public int InactiveOtherNumber { get; set; }
        public int InactiveECarNumber { get; set; }
        public int InactiveEMotorNumber { get; set; }
        public int InactiveEBikeNumber { get; set; }

        public int ExpireCarNumber { get; set; }
        public int ExpireMotorNumber { get; set; }
        public int ExpireBikeNumber { get; set; }
        public int ExpireOtherNumber { get; set; }
        public int ExpireECarNumber { get; set; }
        public int ExpireEMotorNumber { get; set; }
        public int ExpireEBikeNumber { get; set; }
    }
}
