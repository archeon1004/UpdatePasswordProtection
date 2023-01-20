namespace ThreatDetectionModule
{
    internal class Config
    {

        internal bool Enabled { get; set; }
        internal bool BypassPasswordUpdateProtection { get; set; }
        internal int RequestThreshold { get; set; }
        internal bool EnableRiskClaim { get; set; }
        public Config(bool Enabled, bool BypassPasswordUpdateProtection, int RequestThreshold, bool EnableRiskClaim)
        {
            this.Enabled = Enabled;
            this.BypassPasswordUpdateProtection = BypassPasswordUpdateProtection;
            this.RequestThreshold = RequestThreshold;
            this.EnableRiskClaim = EnableRiskClaim;
        }
        public Config()
        {
            this.Enabled = true;
            this.BypassPasswordUpdateProtection = false;
            this.RequestThreshold = 3;
            this.EnableRiskClaim = true;
        }
        public override string ToString()
        {
            return $"Enabled: {this.Enabled};BypassPasswordUpdateProtection: {this.BypassPasswordUpdateProtection};RequestThreshold: {RequestThreshold};EnableRiskClaim: {EnableRiskClaim}";
        }
    }
}
