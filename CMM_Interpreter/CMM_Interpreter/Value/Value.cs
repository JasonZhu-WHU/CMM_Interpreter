using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM_Interpreter
{
    class Value
    {

        public string type;
        public bool initialized;
        public int line_num;
        public bool is_null = false;

        public static string[] computable_types = { "int", "real", "number" };

        //falsey的几种情况，根据C语言测试： \0的char类型，0的int/real，然后我再加一条string为""时，数组均返回true
        public bool getBoolean()
        {
            if(type == "char")
            {
                CharValue v = (CharValue)this;
                return v.value != "\0";
            }
            else if(type == "string")
            {
                StringValue v = (StringValue)this;
                return v.value != "";
            }
            else if(type == "int")
            {
                IntValue v = (IntValue)this;
                return v.value != 0;
            }
            else if(type == "real")
            {
                RealValue v = (RealValue)this;
                return v.value != 0;
            }
            else if (type.Contains("Array"))
            {
                return true;
            }
            else if(type == "func")
            {
                throw new ExecutorException("类型错误，出现了错误的类型：函数", line_num);
            }
            else if(type == "bool")
            {
                BoolValue v = (BoolValue)this;
                return v.value;
            }
            else
            {
                throw new ExecutorException("文法发生了变化", line_num);
            }
        }

        //因为3>2>0这里3>2会evaluate成1
        public IntValue compareWith(Value v, string op = "")
        {
            //直接比较是否相等
            if (op == "")
            {
                if (this.type != v.type)
                {
                    if (type == "int" && v.type == "number")
                    {
                        IntValue v1 = (IntValue)this;
                        NumberValue v2 = (NumberValue)v;
                        if (v1.value == v2.value)
                        {
                            return new IntValue("int", true, "1", line_num);
                        }
                        else
                        {
                            return new IntValue("int", true, "0", line_num);
                        }
                    }
                    else if (type == "real" && v.type == "number")
                    {
                        RealValue v1 = (RealValue)this;
                        NumberValue v2 = (NumberValue)v;
                        if (v1.value == v2.value)
                        {
                            return new IntValue("int", true, "1", line_num);
                        }
                        else
                        {
                            return new IntValue("int", true, "0", line_num);
                        }
                    }
                    else if (type == "number" && v.type == "int")
                    {
                        NumberValue v1 = (NumberValue)this;
                        IntValue v2 = (IntValue)v;
                        if (v1.value == v2.value)
                        {
                            return new IntValue("int", true, "1", line_num);
                        }
                        else
                        {
                            return new IntValue("int", true, "0", line_num);
                        }
                    }
                    else if (type == "number" && v.type == "real")
                    {
                        NumberValue v1 = (NumberValue)this;
                        RealValue v2 = (RealValue)v;
                        if (v1.value == v2.value)
                        {
                            return new IntValue("int", true, "1", line_num);
                        }
                        else
                        {
                            return new IntValue("int", true, "0", line_num);
                        }
                    }
                    else if (type == "int" && v.type == "real")
                    {
                        IntValue v1 = (IntValue)this;
                        RealValue v2 = (RealValue)v;
                        if (v1.value == v2.value)
                        {
                            return new IntValue("int", true, "1", line_num);
                        }
                        else
                        {
                            return new IntValue("int", true, "0", line_num);
                        }
                    }
                    else if (type == "real" && v.type == "int")
                    {
                        RealValue v1 = (RealValue)this;
                        IntValue v2 = (IntValue)v;
                        if (v1.value == v2.value)
                        {
                            return new IntValue("int", true, "1", line_num);
                        }
                        else
                        {
                            return new IntValue("int", true, "0", line_num);
                        }
                    }
                    else
                    {
                        //类型不同比较直接不同
                        return new IntValue("int", true, "0", line_num);
                    }
                }
                else
                {
                    if (type == "int")
                    {
                        IntValue v1 = (IntValue)this;
                        IntValue v2 = (IntValue)v;
                        if (v1.value == v2.value)
                        {
                            return new IntValue("int", true, "1", line_num);
                        }
                        else
                        {
                            return new IntValue("int", true, "0", line_num);
                        }
                    }
                    else if (type == "real")
                    {
                        RealValue v1 = (RealValue)this;
                        RealValue v2 = (RealValue)v;
                        if (v1.value == v2.value)
                        {
                            return new IntValue("int", true, "1", line_num);
                        }
                        else
                        {
                            return new IntValue("int", true, "0", line_num);
                        }
                    }
                    else if (type == "char")
                    {
                        CharValue v1 = (CharValue)this;
                        CharValue v2 = (CharValue)v;
                        if (v1.value == v2.value)
                        {
                            return new IntValue("int", true, "1", line_num);
                        }
                        else
                        {
                            return new IntValue("int", true, "0", line_num);
                        }
                    }
                    else if (type == "string")
                    {
                        StringValue v1 = (StringValue)this;
                        StringValue v2 = (StringValue)v;
                        if (v1.value == v2.value)
                        {
                            return new IntValue("int", true, "1", line_num);
                        }
                        else
                        {
                            return new IntValue("int", true, "0", line_num);
                        }
                    }
                    else if (type == "bool")
                    {
                        BoolValue v1 = (BoolValue)this;
                        BoolValue v2 = (BoolValue)v;
                        if (v1.value == v2.value)
                        {
                            return new IntValue("int", true, "1", line_num);
                        }
                        else
                        {
                            return new IntValue("int", true, "0", line_num);
                        }
                    }
                    else
                    {
                        throw new ExecutorException("暂不实现数组类型比较", line_num);
                    }
                }
            }
            //若是大小比较，那么就只有real/int这些number可以compare，其他的不能
            else if (computable_types.Contains(v.type) && computable_types.Contains(type))
            {
                double v1 = 0, v2 = 0;
                bool result;
                if(type == "int")
                {
                    v1 = ((IntValue)this).value;
                }
                else if(type == "real")
                {
                    v1 = ((RealValue)this).value;
                }
                else
                {
                    v1 = ((NumberValue)this).value;
                }
                if (v.type == "int")
                {
                    v2 = ((IntValue)v).value;
                }
                else if (v.type == "real")
                {
                    v2 = ((RealValue)v).value;
                }
                else
                {
                    v2 = ((NumberValue)v).value;
                }
                if(op == ">")
                {
                    result = v1 > v2;
                }
                else if(op == "<")
                {
                    result = v1 < v2;
                }
                else if (op == ">=")
                {
                    result = v1 >= v2;
                }
                else if (op == "<=")
                {
                    result = v1 <= v2;
                }
                else
                {
                    throw new ExecutorException("出现了文法中未定义的判断符" + op, line_num);
                }
                if (result)
                {
                    return new IntValue("int", true, "1", line_num);
                }
                else
                {
                    return new IntValue("int", true, "0", line_num);
                }
            }
            else
            {
                throw new ExecutorException("无法直接比较大小的类型", line_num);
            }
        }

        public Value mathCalculate(Value v2, string op)
        {
            if (computable_types.Contains(type) && computable_types.Contains(v2.type))
            {
                if (op == "+")
                {
                    if(type == "int" && v2.type == "int")
                    {
                        return new IntValue("int", true, (((IntValue)this).value + ((IntValue)v2).value).ToString(), line_num);
                    }
                    else if(type == "int" && v2.type == "real")
                    {
                        return new RealValue("real", true, (((IntValue)this).value + ((RealValue)v2).value).ToString(), line_num);
                    }
                    else if (type == "real" && v2.type == "int")
                    {
                        return new RealValue("real", true, (((RealValue)this).value + ((IntValue)v2).value).ToString(), line_num);
                    }
                    else if (type == "real" && v2.type == "real")
                    {
                        return new RealValue("real", true, (((RealValue)this).value + ((RealValue)v2).value).ToString(), line_num);
                    }
                    else if (type == "int" && v2.type == "number")
                    {
                        return new NumberValue("number", true, (((IntValue)this).value + ((NumberValue)v2).value).ToString(), line_num);
                    }
                    else if (type == "real" && v2.type == "number")
                    {
                        return new NumberValue("number", true, (((RealValue)this).value + ((NumberValue)v2).value).ToString(), line_num);
                    }
                    else if (type == "number" && v2.type == "int")
                    {
                        return new NumberValue("number", true, (((NumberValue)this).value + ((IntValue)v2).value).ToString(), line_num);
                    }
                    else if (type == "number" && v2.type == "real")
                    {
                        return new NumberValue("number", true, (((NumberValue)this).value + ((RealValue)v2).value).ToString(), line_num);
                    }
                    else if (type == "number" && v2.type == "number")
                    {
                        return new NumberValue("number", true, (((NumberValue)this).value + ((NumberValue)v2).value).ToString(), line_num);
                    }
                    else
                    {
                        throw new ExecutorException("Impossible Situtation!", line_num);
                    }
                }
                else if (op == "-")
                {
                    if (type == "int" && v2.type == "int")
                    {
                        return new IntValue("int", true, (((IntValue)this).value - ((IntValue)v2).value).ToString(), line_num);
                    }
                    else if (type == "int" && v2.type == "real")
                    {
                        return new RealValue("real", true, (((IntValue)this).value - ((RealValue)v2).value).ToString(), line_num);
                    }
                    else if (type == "real" && v2.type == "int")
                    {
                        return new RealValue("real", true, (((RealValue)this).value - ((IntValue)v2).value).ToString(), line_num);
                    }
                    else if (type == "real" && v2.type == "real")
                    {
                        return new RealValue("real", true, (((RealValue)this).value - ((RealValue)v2).value).ToString(), line_num);
                    }
                    else if (type == "int" && v2.type == "number")
                    {
                        return new NumberValue("number", true, (((IntValue)this).value - ((NumberValue)v2).value).ToString(), line_num);
                    }
                    else if (type == "real" && v2.type == "number")
                    {
                        return new NumberValue("number", true, (((RealValue)this).value - ((NumberValue)v2).value).ToString(), line_num);
                    }
                    else if (type == "number" && v2.type == "int")
                    {
                        return new NumberValue("number", true, (((NumberValue)this).value - ((IntValue)v2).value).ToString(), line_num);
                    }
                    else if (type == "number" && v2.type == "real")
                    {
                        return new NumberValue("number", true, (((NumberValue)this).value - ((RealValue)v2).value).ToString(), line_num);
                    }
                    else if (type == "number" && v2.type == "number")
                    {
                        return new NumberValue("number", true, (((NumberValue)this).value - ((NumberValue)v2).value).ToString(), line_num);
                    }
                    else
                    {
                        throw new ExecutorException("Impossible Situtation!", line_num);
                    }
                }
                else if (op == "*")
                {
                    if (type == "int" && v2.type == "int")
                    {
                        return new IntValue("int", true, (((IntValue)this).value * ((IntValue)v2).value).ToString(), line_num);
                    }
                    else if (type == "int" && v2.type == "real")
                    {
                        return new RealValue("real", true, (((IntValue)this).value * ((RealValue)v2).value).ToString(), line_num);
                    }
                    else if (type == "real" && v2.type == "int")
                    {
                        return new RealValue("real", true, (((RealValue)this).value * ((IntValue)v2).value).ToString(), line_num);
                    }
                    else if (type == "real" && v2.type == "real")
                    {
                        return new RealValue("real", true, (((RealValue)this).value * ((RealValue)v2).value).ToString(), line_num);
                    }
                    else if (type == "int" && v2.type == "number")
                    {
                        return new NumberValue("number", true, (((IntValue)this).value * ((NumberValue)v2).value).ToString(), line_num);
                    }
                    else if (type == "real" && v2.type == "number")
                    {
                        return new NumberValue("number", true, (((RealValue)this).value * ((NumberValue)v2).value).ToString(), line_num);
                    }
                    else if (type == "number" && v2.type == "int")
                    {
                        return new NumberValue("number", true, (((NumberValue)this).value * ((IntValue)v2).value).ToString(), line_num);
                    }
                    else if (type == "number" && v2.type == "real")
                    {
                        return new NumberValue("number", true, (((NumberValue)this).value * ((RealValue)v2).value).ToString(), line_num);
                    }
                    else if (type == "number" && v2.type == "number")
                    {
                        return new NumberValue("number", true, (((NumberValue)this).value * ((NumberValue)v2).value).ToString(), line_num);
                    }
                    else
                    {
                        throw new ExecutorException("Impossible Situtation!", line_num);
                    }
                }
                else if (op == "/")
                {
                    try
                    {
                        //只有整数除整数才需要考虑舍入问题，而且C#与C的处理大致相同，所以这里先按这样写
                        if (type == "int" && v2.type == "int")
                        {
                            return new IntValue("int", true, (((IntValue)this).value / ((IntValue)v2).value).ToString(), line_num);
                        }
                        else if (type == "int" && v2.type == "real")
                        {
                            return new RealValue("real", true, (((IntValue)this).value / ((RealValue)v2).value).ToString(), line_num);
                        }
                        else if (type == "real" && v2.type == "int")
                        {
                            return new RealValue("real", true, (((RealValue)this).value / ((IntValue)v2).value).ToString(), line_num);
                        }
                        else if (type == "real" && v2.type == "real")
                        {
                            return new RealValue("real", true, (((RealValue)this).value / ((RealValue)v2).value).ToString(), line_num);
                        }
                        else if (type == "int" && v2.type == "number")
                        {
                            return new NumberValue("number", true, (((IntValue)this).value / ((NumberValue)v2).value).ToString(), line_num);
                        }
                        else if (type == "real" && v2.type == "number")
                        {
                            return new NumberValue("number", true, (((RealValue)this).value / ((NumberValue)v2).value).ToString(), line_num);
                        }
                        else if (type == "number" && v2.type == "int")
                        {
                            return new NumberValue("number", true, (((NumberValue)this).value / ((IntValue)v2).value).ToString(), line_num);
                        }
                        else if (type == "number" && v2.type == "real")
                        {
                            return new NumberValue("number", true, (((NumberValue)this).value / ((RealValue)v2).value).ToString(), line_num);
                        }
                        else if (type == "number" && v2.type == "number")
                        {
                            return new NumberValue("number", true, (((NumberValue)this).value / ((NumberValue)v2).value).ToString(), line_num);
                        }
                        else
                        {
                            throw new ExecutorException("Impossible Situtation!", line_num);
                        }
                    }
                    catch(Exception ee)
                    {
                        throw new ExecutorException("发生除0错误", line_num);
                    }
                }
                else
                {
                    throw new ExecutorException("未识别的运算符", line_num);
                }
            }
            else
            {
                //尽量不要用这个，因为value的linenum不一定可靠（标识符）
                throw new ExecutorException("不可进行数值运算的类型", this.line_num);
            }
        }

        public int getArrayLen()
        {
            if (!this.type.Contains("Array"))
            {
                throw new ExecutorException("该元素并不是一个数组", line_num);
            }
            else
            {
                if (this.type.Contains("int"))
                {
                    IntArrayValue array = (IntArrayValue)this;
                    return array.array_elements.Length;
                }
                else if (this.type.Contains("char"))
                {
                    CharArrayValue array = (CharArrayValue)this;
                    return array.array_elements.Length;
                }
                else if (this.type.Contains("string"))
                {
                    StringArrayValue array = (StringArrayValue)this;
                    return array.array_elements.Length;
                }
                else if (this.type.Contains("real"))
                {
                    RealArrayValue array = (RealArrayValue)this;
                    return array.array_elements.Length;
                }
                else
                {
                    throw new ExecutorException("究竟是何方妖孽？", line_num);
                }
            }
        }

        public int getIntArrayElement(int index)
        {
            if (!this.type.Contains("Array"))
            {
                throw new ExecutorException("该元素并不是一个数组", line_num);
            }
            else
            {
                if (this.type.Contains("int"))
                {
                    IntArrayValue array = (IntArrayValue)this;
                    return array.array_elements.Length;
                }
                else if (this.type.Contains("char"))
                {
                    CharArrayValue array = (CharArrayValue)this;
                    return array.array_elements.Length;
                }
                else if (this.type.Contains("string"))
                {
                    StringArrayValue array = (StringArrayValue)this;
                    return array.array_elements.Length;
                }
                else if (this.type.Contains("real"))
                {
                    RealArrayValue array = (RealArrayValue)this;
                    return array.array_elements.Length;
                }
                else
                {
                    throw new ExecutorException("究竟是何方妖孽？", line_num);
                }
            }
        }

        public string getCharArrayElement(int index)
        {
            if (!this.type.Contains("Array"))
            {
                throw new ExecutorException("该元素并不是一个数组", line_num);
            }
            else
            {
                if (this.type.Contains("char"))
                {
                    CharArrayValue array = (CharArrayValue)this;
                    return array.array_elements[index];
                }
                else
                {
                    throw new ExecutorException("元素类型不符", line_num);
                }
            }
        }

        public string getStringArrayElement(int index)
        {
            if (!this.type.Contains("Array"))
            {
                throw new ExecutorException("该元素并不是一个数组", line_num);
            }
            else
            {
                if (this.type.Contains("string"))
                {
                    StringArrayValue array = (StringArrayValue)this;
                    return array.array_elements[index];
                }
                else
                {
                    throw new ExecutorException("元素类型不符", line_num);
                }
            }
        }

        public void changeValueOfArray(int index, string value)
        {
            if (!type.Contains("Array"))
            {
                throw new ExecutorException("该变量并不是数组！");
            }
            else
            {
                if (type.Contains("int"))
                {
                    IntArrayValue array = (IntArrayValue)this;
                    if (array.array_elements.Length <= index)
                    {
                        throw new ExecutorException("数组访问越界", line_num);
                    }
                    else
                    {
                        try
                        {
                            array.array_elements[index] = int.Parse(value);
                        }
                        catch(Exception ee)
                        {
                            throw new ExecutorException("该类型的值无法为int类型赋值", line_num);
                        }
                    }
                }
                else if (type.Contains("real"))
                {
                    RealArrayValue array = (RealArrayValue)this;
                    if (array.array_elements.Length <= index)
                    {
                        throw new ExecutorException("数组访问越界", line_num);
                    }
                    else
                    {
                        try
                        {
                            array.array_elements[index] = double.Parse(value);
                        }
                        catch (Exception ee)
                        {
                            throw new ExecutorException("该类型的值无法为real类型赋值", line_num);
                        }
                    }
                }
                else if (type.Contains("char"))
                {
                    CharArrayValue array = (CharArrayValue)this;
                    if (array.array_elements.Length <= index)
                    {
                        throw new ExecutorException("数组访问越界", line_num);
                    }
                    else
                    {
                        if (value.Length > 1 && !value.Contains("\\"))
                        {
                            throw new ExecutorException("值" + value + "与类型" + type + "不符合", line_num);
                        }
                        else
                        {
                            array.array_elements[index] = value;
                        }
                    }
                }
                else if (type.Contains("string"))
                {
                    StringArrayValue array = (StringArrayValue)this;
                    if (array.array_elements.Length <= index)
                    {
                        throw new ExecutorException("数组访问越界", line_num);
                    }
                    else
                    {
                        array.array_elements[index] = value;
                    }
                }
            }
        }

        public double getRealArrayElement(int index)
        {
            if (!this.type.Contains("Array"))
            {
                throw new ExecutorException("该元素并不是一个数组", line_num);
            }
            else
            {
                if (this.type.Contains("real"))
                {
                    RealArrayValue array = (RealArrayValue)this;
                    return array.array_elements[index];
                }
                else
                {
                    throw new ExecutorException("元素类型不符", line_num);
                }
            }
        }

        public string getString()
        {
            if(type == "int")
            {
                return ((IntValue)this).value.ToString();
            }
            else if (type == "real")
            {
                return ((RealValue)this).value.ToString();
            }
            else if (type == "number")
            {
                return ((NumberValue)this).value.ToString();
            }
            else if (type == "char")
            {
                return ((CharValue)this).value.ToString();
            }
            else if (type == "string")
            {
                return ((StringValue)this).value.ToString();
            }
            else
            {
                throw new ExecutorException("不应该调用此方法用于这些类型");
            }
        }

    }
}
