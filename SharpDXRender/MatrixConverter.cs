namespace SharpDXRender
{
    using SharpDX;
    using WinPoint = System.Windows.Point;
    public class MatrixConverter
    {
        public static WinPoint ApplyMatrixPoint(Matrix3x2 transform, float x, float y)
        {
            Vector2 renderScopePos = Matrix3x2.TransformPoint(transform, new Vector2(x, y));
            return new WinPoint(renderScopePos.X, renderScopePos.Y);
        }
        public static WinPoint ApplyMatrixPoint(Matrix3x2 transform, WinPoint pt)
        {
            Vector2 renderScopePos = Matrix3x2.TransformPoint(transform, new Vector2((float)pt.X, (float)pt.Y));
            return new WinPoint(renderScopePos.X, renderScopePos.Y);
        }
        public static WinPoint InvertMatrixPoint(Matrix3x2 transform, WinPoint pt)
        {
            Matrix3x2 inverse = Matrix3x2.Invert(transform);
            Vector2 renderScopePos = Matrix3x2.TransformPoint(inverse, new Vector2((float)pt.X, (float)pt.Y));
            return new WinPoint(renderScopePos.X, renderScopePos.Y);
        }
    }
}
