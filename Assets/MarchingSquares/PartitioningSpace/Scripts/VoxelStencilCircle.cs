namespace MarchingSquares.PartitioningSpace.Scripts
{
    public class VoxelStencilCircle : VoxelStencil
    {
        private int _sqrRadius;

        public override void Initialize(bool fillType, int radius)
        {
            base.Initialize(fillType, radius);
            _sqrRadius = radius * radius;
        }

        public override bool Apply(int x, int y, bool voxel)
        {
            x -= CenterX;
            y -= CenterY;
            if (x * x + y * y <= _sqrRadius)
            {
                return FillType;
            }

            return voxel;
        }
    }
}