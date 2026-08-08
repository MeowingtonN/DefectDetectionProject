using HalconDotNet;

namespace Wang.DefectDetectionProject.Shared.Controls
{
    public enum ShapeType
    {
        Rectangle,
        Ellipse,
        Circle,
        Region
    }

    /// <summary>
    /// 绘制的图形的信息
    /// </summary>
    public class DrawingObjectInfo
    {
        public ShapeType ShapeType { get; set; }
        public HTuple?[]? HTuples { get; set; }

        public HObject? Hobject { get; set; }
    }
}
