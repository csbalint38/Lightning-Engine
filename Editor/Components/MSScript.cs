namespace Editor.Components;

sealed class MSScript : MSComponent<Script>
{
    private string? _name;

    public string? Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    public MSScript(MSEntityBase msEntity) : base(msEntity)
    {
        Refresh();
    }

    protected override bool UpdateComponents(string propertyName)
    {
        if (propertyName == nameof(Name) && _name is not null)
        {
            SelectedComponents.ForEach(c => c.Name = _name);
            return true;
        }

        return false;
    }

    protected override bool UpdateMSComponent()
    {
        Name = MSEntity.GetMixedValue(SelectedComponents, new Func<Script, string>(x => x.Name));

        return true;
    }
}
