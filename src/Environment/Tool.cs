using System.ComponentModel;

namespace Environment
{
    public enum Tool
    {
        None,
        Add,
        [Description("⬆/⬇")]
        TerrainUpDown,
        Disaster,
        Move,
        Terraform,
        Inspect,
    }
}
