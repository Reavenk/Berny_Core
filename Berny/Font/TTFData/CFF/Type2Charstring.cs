using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Berny
    {
        namespace CFF
        {
            /// <summary>
            /// The Type 2 format provides a method for compact encoding of
            /// glyph procedures in an outline font program.Type 2 charstrings
            /// must be used in a CFF (Compact Font Format) or OpenType font
            /// file to create a complete font program.
            /// 
            /// https://wwwimages2.adobe.com/content/dam/acom/en/devnet/font/pdfs/5177.Type2.pdf
            /// </summary>
            public class Type2Charstring
            {
                public enum Op
                {
                    HStem = 1,    // Hint operator
                    VStem = 3,    // Hint operator
                    VMoveTo = 4,
                    RLineTo = 5,
                    HLineTo = 6,
                    VLineTo = 7,
                    RRCurveTo = 8,
                    CallSubr = 10,
                    Return = 11,
                    VSIndex = 15,
                    Blend = 16,
                    DotSection = (12 << 8) | 0, //Deprecated
                    And = (12 << 8) | 3,
                    Or = (12 << 8) | 4,
                    Not = (12 << 8) | 5,
                    Abs = (12 << 8) | 9,
                    Add = (12 << 8) | 10,
                    Sub = (12 << 8) | 11,
                    Div = (12 << 8) | 12,
                    Neg = (12 << 8) | 14,
                    Eq = (12 << 8) | 15,
                    Drop = (12 << 8) | 18,
                    Put = (12 << 8) | 20,
                    Get = (12 << 8) | 21,
                    Ifelse = (12 << 8) | 22,
                    Random = (12 << 8) | 23,
                    Mul = (12 << 8) | 24,
                    Sqrt = (12 << 8) | 26,
                    Dup = (12 << 8) | 27,
                    Exch = (12 << 8) | 28,
                    Index = (12 << 8) | 29,
                    Roll = (12 << 8) | 30,
                    Flex = (12 << 8) | 35,
                    Flex1 = (12 << 8) | 37,
                    HFlex = (12 << 8) | 34,
                    HFlex1 = (12 << 36) | 36,
                    EndChar = 14,
                    HStemHM = 18,   // Hint operator
                    HintMask = 19,   // + mask byte  - Hint operator
                    CntrMask = 20,   // + mask byte  - Hint operator
                    VStemHM = 23,   // Hint operator
                    RMoveTo = 21,
                    HMoveTo = 22,
                    RCurveLine = 24,
                    RLineCurve = 25,
                    VVCurveTo = 26,
                    HHCurveTo = 27,
                    CallGSubR = 29,
                    VHCurveTo = 30,
                    HVCurveTo = 31,
                    Reserved_Store = (12 << 8)|8,
                    Reserved_Load = (12 << 8)|13
                }

                // If true, do checks while executing to make sure the 
                // program is well formed.
                const bool validateContext = true;
                byte [] program;

                public Type2Charstring()
                { }

                public Type2Charstring(byte [] program)
                { 
                    this.program = program;
                }

                public Font.Glyph ExecuteProgram()
                {
                    Dictionary<int, Type2Charstring> noSubsLocal = 
                        new Dictionary<int, Type2Charstring>();

                    Dictionary<int, Type2Charstring> noSubsGlobal =
                        new Dictionary<int, Type2Charstring>();

                    return ExecuteProgram(this.program, noSubsLocal, noSubsGlobal);
                }

                public static Font.Glyph ExecuteProgram(
                    byte [] program,
                    Dictionary<int, Type2Charstring> localSubs,
                    Dictionary<int, Type2Charstring> globalSubs)
                {
                    // The program stack seems to require some "flexibility" so
                    // we forgo an actual stack
                    List<Operand> stack = new List<Operand>();

                    Font.Glyph ret = new Font.Glyph();
                    Vector2 lastPos = Vector2.zero;

                    ExecContext context = 
                        new ExecContext(
                            ret,
                            localSubs, 
                            globalSubs);

                    if(ExecuteProgram(program, ref context) == false)
                        throw new System.Exception("Type2Charstring ended program without endchar.");

                    return ret;
                }

                public static bool ExecuteProgram(
                    byte[] program,
                    ref ExecContext context)
                {
                    TTF.TTFReaderBytes instructionPtr = new TTF.TTFReaderBytes(program);
                    if (ExecuteProgram(instructionPtr, ref context) == false)
                        throw new System.Exception("Type2Charstring ended program without endchar.");

                    return true;
                }

                /// <summary>
                /// Subroutine execution of a Type 2 Charstring. This also includes controlling the 
                /// execution of the main routine.
                /// </summary>
                /// <returns>
                /// If true, endchar has been called - else, execution ended from calling return. If
                /// the end of the program is reached without a return or endchar opcode, an exception
                /// is thrown.
                /// </returns>
                public static bool ExecuteProgram(
                    TTF.TTFReaderBytes instructionPtr,
                    ref ExecContext ctx)
                {
                    // Page 10 of 
                    // https://www.adobe.com/content/dam/acom/en/devnet/font/pdfs/5177.Type2.pdf
                    // w ? { hs* vs*cm * hm * mt subpath}? { mt subpath} *endchar
                    // if where opcode instructions start.
                    // 
                    // Where:
                    // w = width
                    // hs = hstem or hstemhm command
                    // vs = vstem or vstemhm command
                    // cm = cntrmask operator
                    // hm = hintmask operator
                    // mt = moveto(i.e.any of the moveto) operators
                    //
                    // subpath = refers to the construction of a subpath(one
                    // complete closed contour), which may include hintmask
                    // operators where appropriate.
                    // and the following symbols indicate specific usage:
                    // *zero or more occurrences are allowed
                    // ? zero or one occurrences are allowed
                    // + one or more occurrences are allowed
                    // { } indicates grouping

                    // The transient array provides non-persistent storage for intermediate values. There is no 
                    // provision to initialize this array, except explicitly using the put operator, and values 
                    // stored in the array do not persist beyond the scope of rendering an individual character.
                    List<Operand> transient = new List<Operand>();

                    if(ctx.atStart == true)
                    {
                        ctx.atStart = false;

                        Operand opFirst = Operand.ReadType2Op(instructionPtr);
                        ctx.width = opFirst.GetReal();

                        // relative to NominalWidthX in the CFF file
                        ctx.glyph.advance = ctx.width; 
                    }

                    while (instructionPtr.AtEnd() == false)
                    { 
                        Operand opRead = Operand.ReadType2Op(instructionPtr);

                        if(opRead.type == Operand.Type.Error)
                            throw new System.Exception("Attempting to execute program with an error in it.");

                        if(opRead.type != Operand.Type.Operator)
                        { 
                            ctx.AddOp(opRead);
                            continue;
                        }

                        // If we're here, it's an instruction and we handle the bytecode based on
                        // parameters on the stack.
                        switch(opRead.intVal)
                        {
                            case (int)Op.HStem: // Hint operator
                                // |- y dy {dya dyb}* hstem (1) |-
                                //
                                // Specifies one or more horizontal stem hints (see the following section for more 
                                // information about horizontal stem hints). This allows multiple pairs of numbers, 
                                // limited by the stack depth, to be used as arguments to a single hstem operator.
                                if (validateContext == true && ctx.OpCt() < 2)
                                    throw new System.Exception("HStem requires two parameters");

                                for (int i = 0; i < ctx.OpCt(); ++i)
                                    ctx.hstems.Add(ctx[i].GetReal());

                                ctx.ClearOps();
                                break;

                            case (int)Op.VStem: // Hint operator
                                // |- x dx {dxa dxb}* vstem (3) |-
                                //
                                // Specifies one or more vertical stem hints between the x coordinates x and x+dx, 
                                // where x is relative to the origin of the coordinate axes.
                                if (validateContext == true && ctx.Count < 2)
                                    throw new System.Exception("VStem requires at least two parameters");

                                for (int i = 0; i < ctx.Count; ++i)
                                    ctx.vstems.Add(ctx[i].GetReal());

                                ctx.ClearOps();
                                break;

                            case (int)Op.VMoveTo:
                                // vmoveto |- dy1 vmoveto (4) |-
                                // moves the current point dy1 units in the vertical direction.
                                if (validateContext == true && ctx.Count < 1)
                                    throw new System.Exception("RMoveTo requires two parameters");

                                ctx.AddPosY(ctx.PopReal());
                                ctx.FlagProcessedMoveTo();
                                ctx.StartContour();
                                break;

                            case (int)Op.RLineTo:
                                // rlineto | - { dxa dya} +rlineto(5) | -
                                // appends a line from the current point to a position at the relative coordinates 
                                // dxa, dya.Additional rlineto operations are performed for all subsequent argument 
                                // pairs.The number of lines is determined from the number of arguments on the stack.
                                if (validateContext == true && ctx.Count < 2)
                                    throw new System.Exception("RLineTo requires two parameters");

                                for(int i = 0; i < ctx.Count; i+=2)
                                { 
                                    Vector2 v = 
                                        new Vector2(
                                            ctx[i + 0].GetReal(),
                                            ctx[i + 1].GetReal());

                                    ctx.AddLineDelta(v);
                                }
                                ctx.ClearOps();

                                break;

                            case (int)Op.HLineTo:
                                // |- dx1 {dya dxb}* hlineto (6) |-
                                // - {dxa dyb}+ hlineto (6) |-
                                // appends a horizontal line of length dx1 to the current point. With an odd number of 
                                // arguments, subsequent argument pairs are interpreted as alternating values of dy and dx, 
                                // for which additional lineto operators draw alternating vertical and horizontal lines.
                                // With an even number of arguments, the arguments are interpreted as alternating horizontal 
                                // and vertical lines.The number of lines is determined from the number of arguments on the stack.
                                if (validateContext == true && ctx.Count < 1)
                                    throw new System.Exception("HLineTo requires one  parameter");

                                for( int i = 0; i < ctx.Count; ++i)
                                { 
                                    float f = ctx[i].GetReal();
                                    if(i % 2 == 0)
                                        ctx.AddPosX(f);
                                    else
                                        ctx.AddPosY(f);

                                    ctx.AddLine();
                                }
                                ctx.ClearOps();
                                break;

                            case (int)Op.VLineTo:
                                // - dy1 {dxa dyb}* vlineto (7) |-
                                // - {dya dxb}+ vlineto (7) |-
                                // appends a vertical line of length dy1 to the current point. With an odd number of arguments, subsequent 
                                // argument pairs are interpreted as alternating values of dx and dy, for which additional lineto operators 
                                // draw alternating horizontal and vertical lines.With an even number of arguments, the arguments are 
                                // interpreted as alternating vertical and horizontal lines.The number of lines is determined from the number 
                                // of arguments on the stack.
                                if (validateContext == true && ctx.Count < 1)
                                    throw new System.Exception("VLineTo requires one parameter");

                                for (int i = 0; i < ctx.Count; ++i)
                                {
                                    float f = ctx[i].GetReal();
                                    if (i % 2 == 0)
                                        ctx.AddPosY(f);
                                    else
                                        ctx.AddPosX(f);

                                    ctx.AddLine();
                                }
                                ctx.ClearOps();
                                break;

                            case (int)Op.RRCurveTo:
                                // rrcurveto | - { dxa dya dxb dyb dxc dyc} +rrcurveto(8) |
                                //
                                // appends a Bézier curve, defined by dxa...dyc, to the current point.For each subsequent set of six arguments, 
                                // an additional curve is appended to the current point.The number of curve segments is determined from the 
                                // number of arguments on the number stack and is limited only by the size of the number stack.
                                if (validateContext == true && ctx.Count < 6)
                                    throw new System.Exception("VLineTo requires six parameters");

                                {
                                    for(int i = 0; i < ctx.Count - 5; i += 6)
                                    {
                                        Vector2 a = ctx.ExtractAddedVector(i + 0);
                                        Vector2 b = ctx.ExtractAddedVector(i + 2);
                                        Vector2 c = ctx.ExtractAddedVector(i + 4);
                                        ctx.Add3Bezier(a, b, c);
                                    }

                                    ctx.ClearOps();
                                }
                                break;

                            case (int)Op.Return:
                                // – return (11) –
                                // returns from either a local or global charstring subroutine, and
                                // continues execution after the corresponding call(g)subr.
                                return false;

                            case (int)Op.VSIndex:
                                Debug.Log("Consume unhandled opcode vsindex.");
                                ctx.PopOp(1);
                                break;

                            case (int)Op.Blend:
                                Debug.Log("Hit opcode vsindex.");
                                break;

                            case (int)Op.DotSection:
                                break;

                            case (int)Op.And:
                                // and num1 num2 and (12 3) 1_or_0
                                // puts a 1 on the stack if num1 and num2 are both non-zero, and
                                // puts a 0 on the stack if either argument is zero.
                                if (validateContext == true && ctx.Count < 2)
                                    throw new System.Exception("And requires six parameters");

                                { 
                                    int lastIdx = ctx.Count - 1;
                                    Operand opa = ctx[lastIdx - 1];
                                    Operand opb = ctx[lastIdx];
                                    ctx.PopOp(2);
                                    // Just get an int, since 0 is the same byte pattern regardless.
                                    bool b = opa.NonZero() && opb.NonZero();
                                    ctx.AddOp(new Operand( b ? 1 : 0));
                                }
                                break;

                            case (int)Op.Or:
                                // or num1 num2 or (12 4) 1_or_0
                                // puts a 1 on the stack if either num1 or num2 are non-zero, and
                                // puts a 0 on the stack if both arguments are zero.
                                if (validateContext == true && ctx.Count < 2)
                                    throw new System.Exception("And requires two parameters.");

                                {
                                    int lastIdx = ctx.Count - 1;
                                    Operand opa = ctx[lastIdx - 1];
                                    Operand opb = ctx[lastIdx];
                                    ctx.PopOp( 2);
                                    // Just get an int, since 0 is the same byte pattern regardless.
                                    bool b = opa.NonZero() || opb.NonZero();
                                    ctx.AddOp(new Operand(b ? 1 : 0));
                                }
                                break;

                            case (int)Op.Not:
                                // not num1 not(12 5) 1_or_0
                                // returns a 0 if num1 is non-zero; returns a 1 if num1 is zero.
                                if (validateContext == true && ctx.Count < 1)
                                    throw new System.Exception("And requires one parameter.");

                                {
                                    int lastIdx = ctx.Count - 1;
                                    Operand opa = ctx[lastIdx];
                                    if(opa.IsZero())
                                        ctx[lastIdx] = new Operand(1);
                                    else
                                        ctx[lastIdx] = new Operand(0);
                                }
                                break;

                            case (int)Op.Abs:
                                // abs num abs (12 9) num2
                                // returns the absolute value of num.
                                if (validateContext == true && ctx.Count < 1)
                                    throw new System.Exception("Abs requires one parameter.");

                                {
                                    int lastIdx = ctx.Count - 1;
                                    Operand op = ctx[lastIdx];
                                    op.intVal = Mathf.Abs(op.intVal);       // Just blindly abs both of them
                                    op.realVal = Mathf.Abs(op.realVal);
                                    ctx[lastIdx] = op;
                                }
                                break;

                            case (int)Op.Add:
                                // add num1 num2 add (12 10) sum
                                // returns the sum of the two numbers num1 and num2.
                                if (validateContext == true && ctx.Count < 2)
                                    throw new System.Exception("Add requires two parameters.");

                                { 
                                    int lastIdx = ctx.Count - 1;
                                    Operand opa = ctx[lastIdx - 1];
                                    Operand opb = ctx[lastIdx];

                                    if(opa.type == Operand.Type.Real || opb.type == Operand.Type.Real)
                                    { 
                                        float f = opa.GetReal() + opb.GetReal();
                                        opa.realVal = f;
                                        opa.intVal = (int)f;
                                    }
                                    else
                                    { 
                                        int n = opa.GetInt() + opb.GetInt();
                                        opa.intVal = n;
                                        opa.realVal = (float)n;
                                    }
                                    ctx[lastIdx - 1] = opa;
                                    ctx.PopOp(1);
                                }
                                break;

                            case (int)Op.Sub:
                                // sub num1 num2 sub (12 11) difference
                                // returns the result of subtracting num2 from num1.
                                if (validateContext == true && ctx.Count < 2)
                                    throw new System.Exception("Sub requires two parameters.");

                                {
                                    int lastIdx = ctx.Count - 1;
                                    Operand opa = ctx[lastIdx - 1];
                                    Operand opb = ctx[lastIdx];

                                    if (opa.type == Operand.Type.Real || opb.type == Operand.Type.Real)
                                    {
                                        float f = opa.GetReal() - opb.GetReal();
                                        opa.realVal = f;
                                        opa.intVal = (int)f;
                                    }
                                    else
                                    {
                                        int n = opa.GetInt() - opb.GetInt();
                                        opa.intVal = n;
                                        opa.realVal = (float)n;
                                    }
                                    ctx[lastIdx - 1] = opa;
                                    ctx.PopOp(1);
                                }
                                break;

                            case (int)Op.Div:
                                // div num1 num2 div (12 12) quotient
                                // returns the quotient of num1 divided by num2. The result is undefined if 
                                // overflow occurs and is zero for underflow.
                                if (validateContext == true && ctx.Count < 2)
                                    throw new System.Exception("Div requires two parameters.");

                                {
                                    int lastIdx = ctx.Count - 1;
                                    Operand opa = ctx[lastIdx - 1];
                                    Operand opb = ctx[lastIdx];

                                    if (opa.type == Operand.Type.Real || opb.type == Operand.Type.Real)
                                    {
                                        float f = opa.GetReal() / opb.GetReal();
                                        opa.realVal = f;
                                        opa.intVal = (int)f;
                                    }
                                    else
                                    {
                                        int n = opa.GetInt() / opb.GetInt();
                                        opa.intVal = n;
                                        opa.realVal = (float)n;
                                    }
                                    ctx[lastIdx - 1] = opa;
                                    ctx.PopOp(1);
                                }
                                break;

                            case (int)Op.Neg:
                                // neg num neg (12 14) num2
                                // returns the negative of num.
                                if (validateContext == true && ctx.Count < 1)
                                    throw new System.Exception("Abs requires one parameter.");

                                {
                                    int lastIdx = ctx.Count - 1;
                                    Operand op = ctx[lastIdx];
                                    op.intVal = -op.intVal;
                                    op.realVal = -op.realVal;
                                    ctx[lastIdx] = op;
                                }
                                break;

                            case (int)Op.Eq:
                                if (validateContext == true && ctx.Count < 2)
                                    throw new System.Exception("Eq requires two parameters.");

                                {
                                    int lastIdx = ctx.Count - 1;
                                    Operand opa = ctx[lastIdx - 1];
                                    Operand opb = ctx[lastIdx];

                                    bool b;
                                    if (opa.type == Operand.Type.Real || opb.type == Operand.Type.Real)
                                        b = opa.GetReal() == opb.GetReal();
                                    else
                                        b = opa.GetInt() == opa.GetReal();
                                    
                                    ctx.PopOp( 2);
                                    ctx.AddOp( new Operand( b ? 1 : 0));
                                }
                                break;

                            case (int)Op.Drop:
                                // drop num drop (12 18)
                                // removes the top element num from the Type 2 argument stack.
                                ctx.PopOp(1);
                                break;

                            case (int)Op.Put:
                                // put val i put (12 20)
                                // stores val into the transient array at the location given by i.
                                if (validateContext == true && ctx.Count < 1)
                                    throw new System.Exception("Put requires two parameters.");

                                {
                                    int lastIdx = ctx.Count - 1;
                                    Operand op = ctx[lastIdx - 1];
                                    int idx = ctx[lastIdx].GetInt();

                                    ctx.PopOp(2);

                                    if(idx >= transient.Count)
                                        transient.Add(op);
                                    else
                                        transient[idx] = op;
                                }
                                break;
                            case (int)Op.Get:
                                // get i get (12 21) val 
                                // retrieves the value stored in the transient array at the location given 
                                // by i and pushes the value onto the argument stack.If get is executed prior 
                                // to put for i during execution of the current charstring, the value 
                                // returned is undefined.
                                if (validateContext == true && ctx.Count < 1)
                                    throw new System.Exception("Get requires one parameter.");

                                { 
                                    int lastIdx = ctx.Count - 1;
                                    int idx = ctx[lastIdx].GetInt();
                                    ctx.PopOp(1);
                                    if(idx >= transient.Count)
                                        ctx[lastIdx] = new Operand(0);

                                    ctx[lastIdx] = transient[idx];
                                }
                                break;

                            case (int)Op.Ifelse:
                                // ifelse s1 s2 v1 v2 ifelse(12 22) s1_or_s2
                                // leaves the value s1 on the stack if v1 ≤ v2, or leaves s2 on the stack if 
                                // v1 > v2.The value of s1 and s2 is usually the biased number of a subroutine;
                                if (validateContext == true && ctx.Count < 4)
                                    throw new System.Exception("IfElse requires four parameters.");

                                { 
                                    int lastIdx = ctx.Count - 1;
                                    Operand s1 = ctx[lastIdx - 4];
                                    Operand s2 = ctx[lastIdx - 3];
                                    Operand v1 = ctx[lastIdx - 2];
                                    Operand v2 = ctx[lastIdx - 1];
                                    ctx.PopOp(4);

                                    if(v1.type == Operand.Type.Real || v2.type == Operand.Type.Real)
                                    { 
                                        float f1 = v1.GetReal();
                                        float f2 = v2.GetReal();

                                        if(f1 < f2)
                                            ctx.AddOp(s1);
                                        else if(f2 < f1)
                                            ctx.AddOp(s2);
                                    }
                                    else
                                    { 
                                        int n1 = v1.GetInt();
                                        int n2 = v2.GetInt();

                                        if(n1 < n2)
                                            ctx.AddOp(s1);
                                        else if(n2 < n1)
                                            ctx.AddOp(s2);
                                    }
                                }
                                break;

                            case (int)Op.Random:
                                // random random (12 23) num2
                                // returns a pseudo random number num2 in the range (0,1], that is, greater 
                                // than zero and less than or equal to one.

                                // The 0 exclusive range makes it somewhat problematic - so we're going to base
                                // it off an int that has a unscaled minimum of 1.
                                ctx.AddOp( new Operand((float)Random.Range(1, short.MaxValue) / (float)short.MaxValue));
                                break;

                            case (int)Op.Mul:
                                // mul num1 num2 mul (12 24) product
                                // returns the product of num1 and num2. If overflow occurs, the
                                // result is undefined, and zero is returned for underflow.
                                if (validateContext == true && ctx.Count < 2)
                                    throw new System.Exception("Mul requires two parameter.");

                                {
                                    int lastIdx = ctx.Count - 1;
                                    Operand opa = ctx[lastIdx - 1];
                                    Operand opb = ctx[lastIdx];
                                    ctx.PopOp(2);

                                    if(opa.type == Operand.Type.Real || opb.type == Operand.Type.Real)
                                        ctx.AddOp(new Operand(opa.GetReal() * opb.GetReal()));
                                    else
                                        ctx.AddOp(new Operand(opa.GetInt() * opb.GetInt()));
                                }
                                break;

                            case (int)Op.Sqrt:
                                // sqrt num sqrt (12 26) num2
                                // returns the square root of num. If num is negative, the result is
                                // undefined.
                                if (validateContext == true && ctx.Count < 1)
                                    throw new System.Exception("Sqrt requires two parameter.");

                                { 
                                    int lastIdx = ctx.Count - 1;
                                    Operand op = ctx[lastIdx];
                                    ctx[lastIdx] = new Operand( Mathf.Sqrt(op.GetReal()));
                                }
                                break;

                            case (int)Op.Dup:
                                // dup any dup (12 27) any any
                                // duplicates the top element on the argument stack.
                                if (validateContext == true && ctx.Count < 1)
                                    throw new System.Exception("Exch requires two parameters.");

                                ctx.AddOp(ctx[ctx.Count - 1]);
                                break;

                            case (int)Op.Exch:
                                // exch num1 num2 exch (12 28) num2 num1
                                // exchanges the top two elements on the argument stack.
                                if (validateContext == true && ctx.Count < 1)
                                    throw new System.Exception("Exch requires two parameters.");

                                {
                                    int lastIdx = ctx.Count - 1;
                                    Operand opa = ctx[lastIdx - 1];
                                    Operand opb = ctx[lastIdx];

                                    ctx[lastIdx - 1] = opb;
                                    ctx[lastIdx] = opa;
                                }
                                break;

                            case (int)Op.Index:
                                // numX ... num0 i index (12 29) numX ... num0 numi
                                //
                                // retrieves the element i from the top of the argument stack and pushes a copy of 
                                // that element onto that stack.If i is negative, the top element is copied.If i is 
                                // greater than X, the operation is undefined.
                                if (validateContext == true && ctx.Count < 1)
                                    throw new System.Exception("Index requires two parameters.");

                                {
                                    int idx = ctx[ctx.Count - 1].GetInt();
                                    ctx.PopOp( 1);

                                    if(idx < 0)
                                        ctx.AddOp( ctx[ctx.Count - 1]);
                                    else if(idx < ctx.Count)
                                        ctx.AddOp(ctx[idx] );
                                    else 
                                        throw new System.Exception("Attempting to access index out of stack bounds.");
                                }
                                break;

                            case (int)Op.Roll:
                                // num(N–1) ... num0 N J roll (12 30) num((J–1) mod N) ... num0 num(N–1)... num(J mod N)
                                //
                                // performs a circular shift of the elements num(N–1) ... num0 on the argument stack by the amount J.
                                // Positive J indicates upward motion of the stack; negative J indicates downward motion. The value N 
                                // must be a non - negative integer, otherwise the operation is undefined.
                                if (validateContext == true && ctx.Count < 1)
                                    throw new System.Exception("Roll requires two parameters.");

                                {
                                    // Not sure if this is correct, couldn't quite decipher the spec
                                    // (wleu 12/26/2020)
                                    int offset = ctx[ctx.Count - 1].GetInt();
                                    ctx.PopOp(0);

                                    List<Operand> oldStk = ctx.stack;
                                    ctx.stack = new List<Operand>();
                                    int num = oldStk.Count;
                                    for( int j = 0; j < oldStk.Count; ++j)
                                    { 
                                        // The +num % num is for handling negative offsets.
                                        ctx.AddOp( oldStk[(((j + offset) %num) + num) % num]);
                                    }
                                }
                                break;

                            case (int)Op.Flex:
                                // flex |- dx1 dy1 dx2 dy2 dx3 dy3 dx4 dy4 dx5 dy5 dx6 dy6 fd flex (12 35) |-
                                // causes two Bézier curves, as described by the arguments, to be rendered as a 
                                // straight line when the flex depth is less than fd / 100 device pixels, and as 
                                // curved lines when the flex depth is greater than or equal to fd / 100 device pixels
                                if (validateContext == true && ctx.Count < 13)
                                    throw new System.Exception("Flex requires thirteen parameters.");

                                // We're going to ignore any complexities and just consume the two beziers.
                                {
                                    int baseIdx = ctx.Count - 13;
                                    Vector2 pt1 = ctx.ExtractAddedVector(baseIdx + 0);
                                    Vector2 pt2 = ctx.ExtractAddedVector(baseIdx + 2);
                                    Vector2 pt3 = ctx.ExtractAddedVector(baseIdx + 4);
                                    Vector2 pt4 = ctx.ExtractAddedVector(baseIdx + 6);
                                    Vector2 pt5 = ctx.ExtractAddedVector(baseIdx + 8);
                                    Vector2 pt6 = ctx.ExtractAddedVector(baseIdx + 10);

                                    ctx.Add6Bezier(pt1, pt2, pt3, pt4, pt5, pt6);
                                    ctx.PopOp(13);
                                }
                                break;

                            case (int)Op.Flex1:
                                // flex1 |- dx1 dy1 dx2 dy2 dx3 dy3 dx4 dy4 dx5 dy5 d6 flex1 (12 37) |-
                                // causes the two curves described by the arguments to be rendered as a straight 
                                // line when the flex depth is less than 0.5 device pixels, and as curved lines 
                                // when the flex depth is greater than or equal to 0.5 device pixels.
                                if (validateContext == true && ctx.Count < 12)
                                    throw new System.Exception("Flex1 requires thirteen parameters.");

                                // We're going to ignore any complexities and just consume the two beziers.
                                {
                                    int baseIdx = ctx.Count - 12;

                                    Vector2 pos = ctx.pos;

                                    Vector2 pt1 = ctx.ExtractAddedVector(baseIdx + 0);
                                    Vector2 pt2 = ctx.ExtractAddedVector(baseIdx + 2);
                                    Vector2 pt3 = ctx.ExtractAddedVector(baseIdx + 4);
                                    Vector2 pt4 = ctx.ExtractAddedVector(baseIdx + 6);
                                    Vector2 pt5 = ctx.ExtractAddedVector(baseIdx + 8);
                                    Vector2 pt6;
                                    float d6 = ctx[baseIdx + 10].GetReal();
                                    float flex = ctx[baseIdx + 11].GetReal();
                                    // We don't currently care about the flex logic, only about extracting
                                    // the curve.
                                    Vector2 diff = pt5 - pt1;
                                    if(Mathf.Abs(diff.y) > Mathf.Abs(diff.x))
                                        pt6 = new Vector2(pos.x, d6);
                                    else
                                        pt6 = new Vector2(d6, pos.x);

                                    pt6 += pt5;
                                    ctx.Add6Bezier(pt1, pt2, pt3, pt4, pt5, pt6);
                                    ctx.PopOp(12);
                                }
                                break;

                            case (int)Op.HFlex:
                                // hflex |- dx1 dx2 dy2 dx3 dx4 dx5 dx6 hflex (12 34) |-
                                // causes the two curves described by the arguments dx1...dx6 to be 
                                // rendered as a straight line when the flex depth is less than 0.5
                                // (that is, fd is 50) device pixels, and as curved lines when the flex 
                                // depth is greater than or equal to 0.5 device pixels. 
                                if (validateContext == true && ctx.Count < 7)
                                    throw new System.Exception("HFlex requires seven parameters.");

                                {
                                    int baseIdx = ctx.Count - 7;

                                    Vector2 pos = ctx.GetPos();
                                    ctx.AddPosX(ctx[baseIdx + 0].GetReal());
                                    Vector2 pt1 = ctx.GetPos();
                                    Vector2 pt2 = ctx.ExtractAddedVector(baseIdx + 1);
                                    ctx.AddPosX(ctx[baseIdx + 3].GetReal());
                                    Vector2 pt3 = ctx.GetPos();
                                    ctx.AddPosX(ctx[baseIdx + 4].GetReal());
                                    Vector2 pt4 = ctx.GetPos();
                                    ctx.AddPosX(ctx[baseIdx + 5].GetReal());
                                    Vector2 pt5 = ctx.GetPos();
                                    ctx.AddPosX(ctx[baseIdx + 6].GetReal());
                                    Vector2 pt6 = ctx.GetPos();

                                    ctx.Add6Bezier(pt1, pt2, pt3, pt4, pt5, pt6);
                                    ctx.PopOp(7);
                                }
                                break;

                            case (int)Op.HFlex1:
                                // hflex1 |- dx1 dy1 dx2 dy2 dx3 dx4 dx5 dy5 dx6 hflex1 (12 36) |-
                                // causes the two curves described by the arguments to be rendered as a straight 
                                // line when the flex depth is less than 0.5 device pixels, and as curved lines 
                                // when the flex depth is greater than or equal to 0.5 device pixels.
                                if (validateContext == true && ctx.Count < 9)
                                    throw new System.Exception("HFlex1 requires seven parameters.");

                                {
                                    int baseIdx = ctx.Count - 9;

                                    Vector2 pt1 = ctx.ExtractAddedVector(baseIdx + 0);
                                    Vector2 pt2 = ctx.ExtractAddedVector(baseIdx + 2);
                                    ctx.AddPosX(ctx[baseIdx + 4].GetReal());
                                    Vector2 pt3 = ctx.GetPos();
                                    ctx.AddPosX(ctx[baseIdx + 5].GetReal());
                                    Vector2 pt4 = ctx.GetPos();
                                    Vector2 pt5 = ctx.ExtractAddedVector(baseIdx + 6);
                                    ctx.AddPosX(ctx[baseIdx + 8].GetReal());
                                    Vector2 pt6 = ctx.GetPos();
                                    
                                    ctx.Add6Bezier(pt1, pt2, pt3, pt4, pt5, pt6);
                                    ctx.PopOp(9);
                                }
                                break;

                            case (int)Op.CallSubr:
                                // subr# callsubr (10) –
                                // calls a charstring subroutine with index subr# (actually the subr
                                // number plus the subroutine bias number
                                if (validateContext == true && ctx.Count < 1)
                                    throw new System.Exception("CallSubr requires one parameter");

                                {
                                    int idx = ctx[ctx.Count - 1].GetInt();
                                    ctx.PopOp(1);

                                    Type2Charstring t2c;
                                    if (ctx.local.TryGetValue(idx, out t2c) == false)
                                        throw new System.Exception("CallSubr attempting to call invalid routine.");

                                    if(ExecuteProgram(t2c.program, ref ctx) == true)
                                        return true;
                                }
                                break;

                            case (int)Op.EndChar:
                                ctx.ended = true;
                                ctx.SealCurrentContour();
                                return true;

                            case (int)Op.HStemHM: // hint operator
                                // hstemhm |- y dy {dya dyb}* hstemhm (18) |-
                                //
                                // has the same meaning as hstem (1), except that it must be used in place of hstem if 
                                // the charstring contains one or more hintmask operators.
                                if (validateContext == true && ctx.Count < 2)
                                    throw new System.Exception("HStemHM requires two parameters");

                                {
                                    for (int i = 0; i < ctx.OpCt(); ++i)
                                        ctx.hstems.Add(ctx[i].GetReal());

                                    ctx.ClearOps();
                                }
                                break;

                            case (int)Op.HintMask: // hint operator
                                // hintmask |- hintmask (19 + mask) |-
                                //
                                // specifies which hints are active and which are not active. If any hints overlap, hintmask 
                                // must be used to establish a nonoverlapping subset of hints. hintmask may occur any number 
                                // of times in a charstring. Path operators occurring after a hintmask are influenced by the 
                                // new hint set, but the current point is not moved.If stem hint zones overlap and are not 
                                // properly managed by use of the hintmask operator, the results are undefined.
                                {
                                    int maskByteCt = ctx.CountMaskBytes();
                                    if(maskByteCt != 0)
                                        ctx.hintMasks = instructionPtr.ReadBytes(maskByteCt);
                                }
                                break;

                            case (int)Op.CntrMask: // hint operator
                                // cntrmask |- cntrmask (20 + mask) |-
                                //
                                // specifies the counter spaces to be controlled, and their relative priority.The mask bits 
                                // in the bytes, following the operator, reference the stem hint declarations; the most significant 
                                // bit of the first byte refers to the first stem hint declared, through to the last hint 
                                // declaration.The counters to be controlled are those that are delimited by the referenced 
                                // stem hints.Bits set to 1 in the first cntrmask command have top priority; subsequent cntrmask 
                                // commands specify lower priority counters
                                if (validateContext == true && ctx.Count < 2)
                                    throw new System.Exception("VStemHM requires two parameters");

                                {
                                    int maskByteCt = ctx.CountMaskBytes();
                                    if (maskByteCt != 0)
                                        ctx.cntrMasks = instructionPtr.ReadBytes(maskByteCt);
                                }
                                break;

                            case (int)Op.VStemHM: // hint operator
                                // vstemhm |- x dx {dxa dxb}* vstemhm (23) |
                                //
                                // has the same meaning as vstem (3), except that it must be used in place of vstem if the charstring 
                                // contains one or more hintmask operators.
                                if (validateContext == true && ctx.Count < 2)
                                    throw new System.Exception("VStemHM requires at least two parameters");

                                for (int i = 0; i < ctx.Count; ++i)
                                    ctx.vstems.Add(ctx[i].GetReal());

                                ctx.ClearOps();
                                break;

                            case (int)Op.RMoveTo:
                                // rmoveto | -dx1 dy1 rmoveto(21) | -
                                //
                                // moves the current point to a position at the relative coordinates (dx1, dy1). 
                                if(validateContext == true && ctx.Count < 2)
                                    throw new System.Exception("RMoveTo requires two parameters");

                                ctx.AddPos(ctx.PopVector());
                                ctx.FlagProcessedMoveTo();
                                ctx.StartContour();
                                break;

                            case (int)Op.HMoveTo:
                                // hmoveto |- dx1 hmoveto (22) |-
                                //
                                // moves the current point dx1 units in the horizontal direction.
                                if (validateContext == true && ctx.Count < 1)
                                    throw new System.Exception("RMoveTo requires two parameters");

                                ctx.AddPosX(ctx.PopReal());
                                ctx.FlagProcessedMoveTo();
                                ctx.StartContour();
                                break;

                            case (int)Op.RCurveLine:
                                // rcurveline |- {dxa dya dxb dyb dxc dyc}+ dxd dyd rcurveline (24) |
                                //
                                // is equivalent to one rrcurveto for each set of six arguments dxa...dyc, followed 
                                // by exactly one rlineto using the dxd, dyd arguments.The number of curves is determined 
                                // from the count on the argument stack.
                                if (validateContext == true && ctx.Count < 8)
                                    throw new System.Exception("RMoveTo requires eight parameters");
                                {
                                    int i = 0;
                                    for(; i < ctx.Count - 2; i += 6)
                                    {
                                        Vector2 a = ctx.ExtractAddedVector(i + 0);
                                        Vector2 b = ctx.ExtractAddedVector(i + 2);
                                        Vector2 c = ctx.ExtractAddedVector(i + 4);
                                        ctx.Add3Bezier(a,b,c);
                                    }

                                    Vector2 d = ctx.ExtractAddedVector(i);
                                    ctx.AddLine(d);
                                    ctx.ClearOps();
                                }
                                break;

                            case (int)Op.RLineCurve:
                                // rlinecurve |- {dxa dya}+ dxb dyb dxc dyc dxd dyd rlinecurve (25) |-
                                //
                                // is equivalent to one rlineto for each pair of arguments beyond the six 
                                // arguments dxb...dyd needed for the one rrcurveto command.The number of 
                                // lines is determined from the count of items on the argument stack.
                                if (validateContext == true && ctx.Count < 8)
                                    throw new System.Exception("RMoveTo requires eight parameters");

                                {
                                    int i = 0;
                                    for(; i + 5 < ctx.Count; i += 2)
                                    { 
                                        Vector2 a = ctx.ExtractAddedVector(i);
                                        ctx.AddLine(a);
                                    }

                                    Vector2 b = ctx.ExtractAddedVector(i); 
                                    i += 2;
                                    Vector2 c = ctx.ExtractAddedVector(i); 
                                    i += 2;
                                    Vector2 d = ctx.ExtractAddedVector(i);

                                    ctx.Add3Bezier(b, c, d);
                                    ctx.ClearOps();
                                }
                                break;

                            case (int)Op.VVCurveTo:
                                // | -dx1 ? { dya dxb dyb dyc} +vvcurveto(26) | -
                                //
                                // Appends one or more curves to the current point. If the argument count is a multiple of 
                                // four, the curve starts and ends vertical. If the argument count is odd, the first curve 
                                // does not begin with a vertical tangent.
                                if (validateContext == true && ctx.Count < 4)
                                    throw new System.Exception("HHCurveTo requires four parameters");

                                {
                                    int i = 0;
                                    if(ctx.Count % 2 == 1)
                                    { 
                                        Vector2 oa = ctx.ExtractAddedVector(0);
                                        Vector2 ob = ctx.ExtractAddedVector(2);
                                        ctx.AddPosY(ctx[4].GetReal());
                                        Vector2 oc = ctx.GetPos();

                                        ctx.Add3Bezier(oa, ob, oc);
                                        i += 5;
                                    }
                                    for(; i + 5 < ctx.Count; i+= 4)
                                    { 
                                        ctx.AddPosY(ctx[ i + 0].GetReal());
                                        Vector2 a = ctx.GetPos();
                                        Vector2 b = ctx.ExtractAddedVector(i + 1);
                                        ctx.AddPosY(ctx[i + 3].GetReal());
                                        Vector2 c = ctx.GetPos();
                                    }
                                    ctx.ClearOps();
                                }
                                break;

                            case (int)Op.HHCurveTo:
                                // hhcurveto |- dy1? {dxa dxb dyb dxc}+ hhcurveto (27) |-
                                //
                                // appends one or more Bézier curves, as described by the dxa...dxc set of arguments, to the current 
                                // point.For each curve, if there are 4 arguments, the curve starts and ends horizontal. The first 
                                // curve need not start horizontal(the odd argument case). Note the argument order for the odd 
                                // argument case.
                                if (validateContext == true && ctx.Count < 1)
                                    throw new System.Exception("HHCurveTo requires four parameters");
                                {
                                    int i = 0;
                                    if (ctx.Count % 2 == 1)
                                    {
                                        float fy = ctx[0].GetReal();
                                        float fx = ctx[1].GetReal();
                                        ctx.AddPos(new Vector2(fx, fy));
                                        Vector2 oa = ctx.GetPos();
                                        Vector2 ob = ctx.ExtractAddedVector(2);
                                        ctx.AddPosX(ctx[4].GetReal());
                                        Vector2 oc = ctx.GetPos();

                                        ctx.Add3Bezier(oa, ob, oc);
                                        i += 5;
                                    }
                                    for (; i + 5 < ctx.Count; i += 4)
                                    {
                                        ctx.AddPosX(ctx[i + 0].GetReal());
                                        Vector2 a = ctx.GetPos();
                                        Vector2 b = ctx.ExtractAddedVector(i + 1);
                                        ctx.AddPosX(ctx[i + 3].GetReal());
                                        Vector2 c = ctx.GetPos();
                                    }
                                    ctx.ClearOps();
                                }
                                break;

                            case (int)Op.CallGSubR:
                                // callgsubr globalsubr# callgsubr (29) –
                                // operates in the same manner as callsubr except that it calls a
                                // global subroutine.
                                if (validateContext == true && ctx.Count < 1)
                                    throw new System.Exception("CallGSubR requires one parameter");

                                {
                                    int idx = ctx[ctx.Count - 1].GetInt();
                                    ctx.PopOp(1);

                                    Type2Charstring t2c;
                                    if(ctx.global.TryGetValue(idx, out t2c) == false)
                                        throw new System.Exception("CallGSubR attempting to call invalid routine.");

                                    if(ExecuteProgram(t2c.program, ref ctx) == true)
                                        return true;
                                }
                                break;

                            case (int)Op.VHCurveTo:
                                // |- dy1 dx2 dy2 dx3 {dxa dxb dyb dyc dyd dxe dye dxf}* dyf? vhcurveto (30) |-
                                // |- {dya dxb dyb dxc dxd dxe dye dyf}+ dxf? vhcurveto (30) |-
                                //
                                // Appends one or more Bézier curves to the current point, where the first tangent is vertical and the 
                                // second tangent is horizontal. This command is the complement of hvcurveto; see the description of 
                                // hvcurveto for more information.
                                if (validateContext == true && ctx.Count < 1)
                                    throw new System.Exception("VHCurveTo requires at least give parameters.");
                                {
                                    // We're going to implement and equivalent pattern of
                                    // {dya, dxb, dyb
                                    int rem8 = ctx.Count % 8;
                                    int i = 0;
                                    if(rem8 == 4 || rem8 == 5)
                                    {
                                        Vector2 pt1 = ctx.AddPosY(ctx[0].GetReal());
                                        Vector2 pt2 = ctx.ExtractAddedVector(1);
                                        Vector2 pt3 = ctx.AddPosX(ctx[3].GetReal());

                                        i = 4;
                                        if(ctx.Count == 5)
                                            pt3.y += ctx[4].GetReal();

                                        ctx.Add3Bezier(pt1, pt2, pt3);

                                        for (; i <= ctx.Count - 8; i += 8)
                                        {
                                            Vector2 a = ctx.AddPosX(ctx[i + 0].GetReal());
                                            Vector2 b = ctx.ExtractAddedVector(1);
                                            Vector2 c = ctx.AddPosY(ctx[i + 3].GetReal());
                                            Vector2 d = ctx.AddPosY(ctx[i + 4].GetReal());
                                            Vector2 e = ctx.ExtractAddedVector(i + 5);
                                            Vector2 f = ctx.AddPosX(ctx[i + 7].GetReal());

                                            // +8 for the normal 8, +1 for 1 addeed extra
                                            if (i + 9 == ctx.Count)
                                                f.y += ctx[8].GetReal();

                                            ctx.Add6Bezier(a, b, c, d, e, f);
                                        }
                                    }
                                    else
                                    {
                                        for (; i <= ctx.Count - 8; i += 8)
                                        {
                                            Vector2 a = ctx.AddPosY(ctx[i + 0].GetReal());
                                            Vector2 b = ctx.ExtractAddedVector(1);
                                            Vector2 c = ctx.AddPosX(ctx[i + 3].GetReal());
                                            Vector2 d = ctx.AddPosX(ctx[i + 4].GetReal());
                                            Vector2 e = ctx.ExtractAddedVector(i + 5);
                                            Vector2 f = ctx.AddPosY(ctx[i + 7].GetReal());

                                            // +8 for the normal 8, +1 for 1 addeed extra
                                            if (i + 9 == ctx.Count)
                                                f.x += ctx[8].GetReal();

                                            ctx.Add6Bezier(a, b, c, d, e, f);
                                        }
                                    }
                                    ctx.ClearOps();
                                }

                                break;

                            case (int)Op.HVCurveTo:
                                // hvcurveto |- dx1 dx2 dy2 dy3 {dya dxb dyb dxc dxd dxe dye dyf}* dxf? hvcurveto(31) | -
                                // |- {dxa dxb dyb dyc dyd dxe dye dxf}+ dyf? hvcurveto (31) |-
                                // 
                                // appends one or more Bézier curves to the current point. The tangent for the first Bézier 
                                // must be horizontal, and the second must be vertical(except as noted below). 
                                //
                                // If there is a multiple of four arguments, the curve starts horizontal and ends vertical. Note 
                                // that the curves alternate between start horizontal, end vertical, and start vertical, and end 
                                // horizontal.The last curve(the odd argument case) need not end horizontal/ vertical.
                                if (validateContext == true && ctx.Count < 1)
                                    throw new System.Exception("HVCurveTo requires at least give parameters.");

                                {
                                    // We're going to implement and equivalent pattern of
                                    // {dya, dxb, dyb
                                    int rem8 = ctx.Count % 8;
                                    int i = 0;
                                    if (rem8 == 4 || rem8 == 5)
                                    {
                                        Vector2 pt1 = ctx.AddPosX(ctx[0].GetReal());
                                        Vector2 pt2 = ctx.ExtractAddedVector(1);
                                        Vector2 pt3 = ctx.AddPosY(ctx[3].GetReal());

                                        i = 4;
                                        if (ctx.Count == 5)
                                            pt3.x += ctx[4].GetReal();

                                        ctx.Add3Bezier(pt1, pt2, pt3);

                                        for (; i <= ctx.Count - 8; i += 8)
                                        {
                                            Vector2 a = ctx.AddPosY(ctx[i + 0].GetReal());
                                            Vector2 b = ctx.ExtractAddedVector(1);
                                            Vector2 c = ctx.AddPosX(ctx[i + 3].GetReal());
                                            Vector2 d = ctx.AddPosX(ctx[i + 4].GetReal());
                                            Vector2 e = ctx.ExtractAddedVector(i + 5);
                                            Vector2 f = ctx.AddPosY(ctx[i + 7].GetReal());

                                            // +8 for the normal 8, +1 for 1 addeed extra
                                            if (i + 9 == ctx.Count)
                                                f.y += ctx[8].GetReal();

                                            ctx.Add6Bezier(a, b, c, d, e, f);
                                        }
                                    }
                                    else
                                    {
                                        for (; i <= ctx.Count - 8; i += 8)
                                        {
                                            Vector2 a = ctx.AddPosX(ctx[i + 0].GetReal());
                                            Vector2 b = ctx.ExtractAddedVector(1);
                                            Vector2 c = ctx.AddPosY(ctx[i + 3].GetReal());
                                            Vector2 d = ctx.AddPosY(ctx[i + 4].GetReal());
                                            Vector2 e = ctx.ExtractAddedVector(i + 5);
                                            Vector2 f = ctx.AddPosX(ctx[i + 7].GetReal());

                                            // +8 for the normal 8, +1 for 1 added extra
                                            if (i + 9 == ctx.Count)
                                                f.y += ctx[8].GetReal();

                                            ctx.Add6Bezier(a, b, c, d, e, f);
                                        }
                                    }
                                    ctx.ClearOps();
                                }

                                ctx.ClearOps();

                                break;

                            // Undocumented and obsolete opcodes.
                            case (int)Op.Reserved_Store:
                            case (int)Op.Reserved_Load:
                                ctx.ClearOps();
                                break;

                            default:
                                throw new System.Exception($"Encountered unknown opcode {opRead.intVal}.");
                        }
                    }
                    throw new System.Exception("Type2Charstring routine finished unexpectedly");
                }
            }
        }
    }
}