
#include "platform/platform.h"
#include "console/console.h"
#include "console/consoleInternal.h"
#include "console/ast.h"
#include "core/resourceManager.h"
#include "core/stream/fileStream.h"
#include "console/compiler.h"
#include "platform/event.h"
//#include "platform/gameInterface.h"
#include "core/hash/md5file.h"

static const char Base16Values[] = "0123456789ABCDEF";

ConsoleFunction(getFileMD5, const char*, 2, 2, "getFileMD5(file)")
{
	char* fileMD5;
	char* returnBuffer = Con::getReturnBuffer( 256 );

	fileMD5 = MD5File( argv[1] ); 
	dSprintf( returnBuffer, 256, "%s", fileMD5 );

	dFree(fileMD5);
	return returnBuffer;
}

ConsoleFunction(getStringMD5, const char*, 2, 2, "getStringMD5(string)")
{
	char* strMD5;
	char* returnBuffer = Con::getReturnBuffer( 256 );

	strMD5 = MD5String( argv[1] ); 
	dSprintf( returnBuffer, 256, "%s", strMD5 );

	dFree(strMD5);
	return returnBuffer;
}