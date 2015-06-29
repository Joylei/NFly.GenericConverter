using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace NFly.GenericConverter.Tests
{
    public class ConverterTests
    {
        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        public void Cast_int_to_string(int src)
        {
            string expected = src.ToString();
            string result = Converter.Convert<string>(src);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("1")]
        [InlineData("2")]
        public void Cast_string_to_int(string src)
        {
            int expected = int.Parse(src);
            int result = Converter.Convert<int>(src);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Cast_guid_to_string()
        {
            Guid id = new Guid();
            string result = Converter.Convert<string>(id);
            Assert.Equal(id.ToString(), result);
        }

        [Fact]
        public void Cast_string_to_guid()
        {
            Guid id = new Guid();
            string src = id.ToString();
            Guid result = Converter.Convert<Guid>(src);
            Assert.Equal(id, result);
        }

        public class BoolTests
        {

            [Theory]
            [InlineData(true)]
            [InlineData(false)]
            public void Cast_bool_to_string(bool src)
            {
                string expected = src.ToString();
                string result = Converter.Convert<string>(src);
                Assert.Equal(expected, result);
            }

            [Theory]
            [InlineData("true")]
            [InlineData("false")]
            public void Cast_string_to_bool(string src)
            {

                bool expected = bool.Parse(src);
                bool result = Converter.Convert<bool>(src);
                Assert.Equal(expected, result);
            }

            [Theory]
            [InlineData(true)]
            [InlineData(false)]
            public void Cast_bool_to_int(bool src)
            {
                int expected = src ? 1 : 0;
                int result = Converter.Convert<int>(src);
                Assert.Equal(expected, result);
            }

            [Theory]
            [InlineData(0)]
            [InlineData(1)]
            [InlineData(2)]
            public void Cast_int_to_bool(int src)
            {
                bool expected = src > 0;
                bool result = Converter.Convert<bool>(src);
                Assert.Equal(expected, result);
            }
        }

        public class NullableTests
        {
            [Fact]
            public void Cast_null_to_nullable()
            {
                int? expected = null;
                int? result = Converter.Convert<int?>(null);
                Assert.Equal(expected, result);
            }

            [Theory]
            [InlineData("5")]
            [InlineData("6")]
            public void Cast_notnull_to_nullable(string src)
            {
                int? expected = int.Parse(src);
                int? result = Converter.Convert<int?>(src);
                Assert.Equal(expected, result);
            }

            [Fact]
            public void Cast_nullable_to_value()
            {
                int? src = 5;
                int expected = src.Value;
                int result = Converter.Convert<int>(src);
                Assert.Equal(expected, result);
            }
        }


        public class EnumTests
        {
            public enum MyEnum
            {
                None = 0,
                Debug = 1,
                Release = 2
            }

            [Fact]
            public void Cast_int_to_enum()
            {
                var src = 2;
                var expected = (MyEnum)src;
                var result = Converter.Convert<MyEnum>(src);
                Assert.Equal(expected, result);
            }

            [Fact]
            public void Cast_enum_to_int()
            {
                var src = MyEnum.Debug;
                var expected = (int)src;
                var result = Converter.Convert<int>(src);
                Assert.Equal(expected, result);
            }

            [Fact]
            public void Cast_enum_to_string()
            {
                var src = MyEnum.Debug;
                var expected = (int)src;
                var result = Converter.Convert<int>(src);
                Assert.Equal(expected, result);
            }

            [Fact]
            public void Cast_string_to_enum()
            {

                var expected = MyEnum.Release;
                var src = expected.ToString();
                var result = Converter.Convert<MyEnum>(src);
                Assert.Equal(expected, result);
            }

            [Fact]
            public void Cast_intstring_to_enum()
            {

                var expected = MyEnum.Release;
                var src = ((int)expected).ToString();
                var result = Converter.Convert<MyEnum>(src);
                Assert.Equal(expected, result);
            }
        }

        public class AssginableTests
        {
            [Fact]
            public void Cast_same_type()
            {
                var src = new MySubClass();
                MySubClass expected = src;
                var result = Converter.Convert<MyClass>(src);
                Assert.Equal(expected, result);
            }

            [Fact]
            public void Cast_sub_to_super()
            {
                var src = new MySubClass();
                var expected = (MyClass)src;
                var result = Converter.Convert<MyClass>(src);
                Assert.Equal(expected, result);
            }

            [Fact]
            public void Cast_super_to_sub()
            {
                var expected = new MySubClass();
                var src = (MyClass)expected;
                var result = Converter.Convert<MySubClass>(src);
                Assert.Equal(expected, result);
            }

            public class MyClass
            {

            }

            public class MySubClass : MyClass
            {

            }
        }


        public class OperatorTests
        {
            [Fact]
            public void Cast_ImplicitModel_to_int()
            {
                var src = new ImplicitModel() { result = 10 };

                var expected = src.result;
                var result = Converter.Convert<int>(src);
                Assert.Equal(expected, result);
            }

            [Fact]
            public void Cast_int_to_ImplicitModel()
            {
                var src = 10;
                var expected = src;
                var target = Converter.Convert<ImplicitModel>(src);
                var result = target.result;
                Assert.Equal(expected, result);
            }

            [Fact]
            public void Cast_ExplicitModel_to_int()
            {
                var src = new ExplicitModel() { result = 10 };

                var expected = src.result;
                var result = Converter.Convert<int>(src);
                Assert.Equal(expected, result);
            }

            [Fact]
            public void Cast_int_to_ExplicitModel()
            {
                var src = 10;
                var expected = src;
                var target = Converter.Convert<ExplicitModel>(src);
                var result = target.result;
                Assert.Equal(expected, result);
            }


            public class ImplicitModel
            {
                public int result = 0;

                public static implicit operator ImplicitModel(int result)
                {
                    return new ImplicitModel(){result = result};
                }

                public static implicit operator int(ImplicitModel first)
                {
                    return first != null ? first.result : 0;
                }
            }

            public class ExplicitModel
            {
                public int result = 0;

                public static explicit operator ExplicitModel(int result)
                {
                    return new ExplicitModel() { result = result };
                }

                public static explicit operator int(ExplicitModel first)
                {
                    return first != null ? first.result : 0;
                }
            }
        }
    }
}
