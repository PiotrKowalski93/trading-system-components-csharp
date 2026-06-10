namespace MemoryPool.Lib.Tests
{
    public class MemoryPoolTests
    {
        [Fact]
        public unsafe void Allocate_ShouldStoreValueInMemory()
        {
            using var pool = new MemoryPool<int>(5);
            int* ptr = pool.Allocate(42);

            Assert.Equal(42, *ptr);
        }

        [Fact]
        public unsafe void Deallocate_ShouldReturnSlotToPool()
        {
            using var pool = new MemoryPool<int>(5);
            int* ptr = pool.Allocate(42);

            Assert.Equal(42, *ptr);
            Assert.Equal(4, pool.FreeCount);

            pool.Deallocate(ptr);
            Assert.Equal(5, pool.FreeCount);
        }

        [Fact]
        public unsafe void Allocate_ShouldThrowWhenPoolIsFull()
        {
            using var pool = new MemoryPool<int>(2);
            pool.Allocate(1);
            pool.Allocate(2);
            Assert.Throws<InvalidOperationException>(() => pool.Allocate(3));
        }

        [Fact]
        public unsafe void Allocate_ShouldReuseDeallocatedSlots()
        {
            using var pool = new MemoryPool<int>(2);
            int* ptr1 = pool.Allocate(1);
            int* ptr2 = pool.Allocate(2);

            Assert.Equal(1, *ptr1);
            Assert.Equal(2, *ptr2);
            Assert.Equal(0, pool.FreeCount);

            pool.Deallocate(ptr1);

            Assert.Equal(1, pool.FreeCount);

            int* ptr3 = pool.Allocate(3);

            Assert.Equal(3, *ptr3);
            Assert.Equal(2, *ptr2);
            Assert.Equal(0, pool.FreeCount);
        }

        [Fact]
        public unsafe void Deallocate_ShouldThrowWhenPointerIsNull()
        {
            using var pool = new MemoryPool<int>(5);
            int* ptr = default;

            Assert.Throws<ArgumentNullException>(() => pool.Deallocate(ptr));
        }

        [Fact]
        public unsafe void Deallocate_ShouldThrowWhenPointerIsNotFromPool()
        {
            using var pool = new MemoryPool<int>(5);
            int a = 5;
            int* ptr = &a;

            Assert.Throws<ArgumentOutOfRangeException>(() => pool.Deallocate(ptr));
        }

        [Fact]
        public unsafe void Deallocate_ShouldClearMemory()
        {
            using var pool = new MemoryPool<int>(3);
            int* ptr = pool.Allocate(123);
            Assert.Equal(123, *ptr);

            pool.Deallocate(ptr);
            Assert.Equal(0, *ptr);
            Assert.Equal(3, pool.FreeCount);
        }

        //TODO: Not covered yet
        //[Fact]
        //public unsafe void Deallocate_ShouldThrowWhenDoubleDeallocating()
        //{
        //    using var pool = new MemoryPool<int>(3);
        //    int* ptr = pool.Allocate(123);

        //    Assert.Equal(123, *ptr);

        //    pool.Deallocate(ptr);
        //    Assert.Throws<ArgumentOutOfRangeException>(() => pool.Deallocate(ptr));
        //}
    }
}
