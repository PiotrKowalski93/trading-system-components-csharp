using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace OrderBook.L3
{
    //TODO: Move to separate project and add tests, add to github page
    /// <summary>
    /// Memory pool for unmanaged types, allowing for efficient allocation and deallocation of memory without the overhead of garbage collection.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public unsafe struct MemoryPool<T> : IDisposable
        where T : unmanaged
    {
        private readonly T* _buffer;
        private readonly int _capacity;

        // Stack of free indices for reuse
        private int* _freeIndexesStack;
        private int _freeTop;

        public MemoryPool(int capacity)
        {
            if(capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be non-negative.");

            _capacity = capacity;

            // Allocate unmanaged memory for the buffer and the free index stack
            _buffer = (T*)NativeMemory.Alloc((nuint)capacity, (nuint)sizeof(T));
            _freeIndexesStack = (int*)NativeMemory.Alloc((nuint)capacity, (nuint)sizeof(int));

            // Initialize the free index stack
            for (int i = 0; i < capacity; i++)
            {
                _freeIndexesStack[i] = capacity - 1 - i;
            }
            _freeTop = capacity;
        }

        private T* GetFreeSlot() 
        {
            if(_freeTop == 0)
                throw new InvalidOperationException("Memory pool is full.");

            // Pop the top index from the free index stack
            int allocateIndex = _freeIndexesStack[--_freeTop];
            T* ptr = _buffer + allocateIndex;

            return ptr;
        }

        public T* Allocate(T value)
        {
            T* ptr = GetFreeSlot();
            *ptr = value;

            return ptr;
        }

        private void Deallocate(T* ptr)
        {
            if(ptr == null)
                throw new ArgumentNullException(nameof(ptr), "Pointer cannot be null.");

            long index = ptr - _buffer;

            if(index < 0 || index >= _capacity)
                throw new ArgumentOutOfRangeException(nameof(ptr), "Pointer is out of bounds of the memory pool.");
            
            *ptr = default;
            _freeIndexesStack[_freeTop++] = (int)index;
        }

        public void Dispose()
        {
            NativeMemory.Free(_buffer);
            NativeMemory.Free(_freeIndexesStack);
        }
    }
}
