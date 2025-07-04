namespace Editor.Components
{
    sealed class MSGeometry : MSComponent<Geometry>
    {
        private GeometryWithMaterials _geometryWithMaterials;

        public GeometryWithMaterials GeometryWithMaterials
        {
            get => _geometryWithMaterials;
            private set
            {
                if (_geometryWithMaterials != value)
                {
                    _geometryWithMaterials = value;
                    OnPropertyChanged(nameof(GeometryWithMaterials));
                }
            }
        }

        public Guid GeometryGuid => _geometryWithMaterials is not null ? SelectedComponents.First().GeometryGuid : Guid.Empty;

        public MSGeometry(MSEntityBase msEntity) : base(msEntity)
        {
            Refresh();
        }

        public void SetGeometry(Guid guid)
        {
            SelectedComponents.ForEach(x => x.SetGeometry(guid));
            Refresh();
        }

        protected override bool UpdateComponents(string propertyName) => false;

        protected override bool UpdateMSComponent()
        {
            var contentId = MSEntity.GetMixedValue(SelectedComponents, new Func<Geometry, IdType>(x => x.ContentId));

            GeometryWithMaterials = contentId.HasValue ? SelectedComponents.First().GeometryWithMaterials : null;

            return true;
        }
    }
}
