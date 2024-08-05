namespace EBrief.Shared.Models.Shared;
public sealed class Either<TLeft, TRight>
{
    private readonly TLeft? _left;
    private readonly TRight? _right;
    public bool IsLeft => _left is not null;
    public bool IsRight => _right is not null;

    public Either(TLeft left) => _left = left;
    public Either(TRight right) => _right = right;
    public static Either<TLeft, TRight> Create(TLeft left) => new(left);
    public static Either<TLeft, TRight> Create(TRight right) =>  new(right);
    public TRight Right()
    {
        if (_right is not null)
        {
            return _right;
        }

        throw new InvalidOperationException();
    }

    public TLeft Left()
    {
        if (_left is not null)
        {
            return _left;
        }

        throw new InvalidOperationException();
    }
}
