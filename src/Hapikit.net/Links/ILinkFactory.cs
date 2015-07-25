namespace Hapikit.Links
{
    public interface ILinkFactory
    {
        ILink CreateLink(string relation);
    }
}