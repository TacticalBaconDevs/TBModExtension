using System;
using System.Text;

namespace TBModExtensionPlugin
{
    public interface ITBModExtensionCommand
    {
        string getName();

        bool isSync();

        int execute(object argument, Action<string, object[]> execCallback, StringBuilder output, long taskId);

    }
}
