public class DataRow : IComparable<DataRow>
{
  public int Number { get; set; }

  public string Text { get; set; }

  public int CompareTo(DataRow? other)
  {
    var res = Text.CompareTo(other.Text);

    if (!Text.Equals(other.Text, StringComparison.InvariantCultureIgnoreCase))
    {
      return res;
    }

    if (Number > other.Number)
    {
      res++;
    }
    else
    {
      res--;
    }

    return res;
  }

  public override string ToString()
  {
    return $"{Number}. {Text}";
  }
}
