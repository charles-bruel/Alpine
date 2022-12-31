using System.Collections.Generic;

public interface IWriteableEnumerator<T> : IEnumerator<T> {
    T CurrentMut { get; set; }
}