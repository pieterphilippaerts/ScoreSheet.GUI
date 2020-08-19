/*
 *   Code gratuitously stolen from my dear friend Frédéric Vogels
 */

namespace PieterP.Shared.Cells {
    public interface IVar<T>
    {
        T Value { get; set; }
    }
}
