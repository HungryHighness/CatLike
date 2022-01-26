namespace MarchingSquares.PartitioningSpace.Scripts
{
    public class VoxelStencil
    {
        protected bool FillType;
        protected int CenterX, CenterY, Radius;

        public int XStart => CenterX - Radius;
        public int XEnd => CenterX + Radius;
        public int YStart => CenterY - Radius;
        public int YEnd => CenterY + Radius;

        public virtual void Initialize(bool fillType, int radius)
        {
            FillType = fillType;
            Radius = radius;
        }

        public virtual void SetCenter(int x, int y)
        {
            CenterX = x;
            CenterY = y;
        }

        public virtual bool Apply(int x, int y, bool voxel)
        {
            return FillType;
        }

    }
}