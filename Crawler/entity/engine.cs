using System;

namespace Crawler.Core{//还有一种写法是namespace加“;”，作用域是整个文件内容
    public record ProgressInfo(int Percent, string Speed, string Message);//record用于简单定义一个只读的class，值相等
}