#include "plugin.h"
#include <game_api.h>
#include <thread>
#include <filesystem>
#include <string>

using namespace std;
using namespace std::filesystem;

unique_ptr<SAMPFUNCS> SF;

static void CALLBACK SendChat(const char* input) {
	SF->getSAMP()->getPlayers()->localPlayerData->Say((char*)input);
}

static void CALLBACK LogToChat(const char* input) {
	SF->getSAMP()->getChat()->AddChatMessage(0xAAAAAA, input);
}

static bool CALLBACK IsPlayerConnected(int playerId) {
	return SF->getSAMP()->getPlayers()->IsPlayerDefined(playerId);
}

static ushort CALLBACK GetAimedPlayerId() {
	return SF->getSAMP()->getPlayers()->localPlayerData->aimingAtPid;
}

static const char* CALLBACK GetPlayerName(int playerId) {
	return SF->getSAMP()->getPlayers()->GetPlayerName(GetAimedPlayerId());
}

static void CALLBACK ShowDialog(ushort dialogId, int dialogStyle, char* dialogCaption, char* dialogLines, char* button1, char* button2) {
	SF->getSAMP()->getDialog()->ShowDialog(dialogId, dialogStyle, dialogCaption, dialogLines, button1, button2);
}

static void CALLBACK RegisterDialogCallback(void(__stdcall* dialogCallback)(int dialogId, int buttonId, int listItem, const char* input)) {
	SF->getSAMP()->registerDialogCallback(dialogCallback);
}

struct CSharpExports {
	void(__stdcall* SendChat)(const char*);
	void(__stdcall* LogToChat)(const char*);
	bool(__stdcall* IsPlayerConnected)(int);
	unsigned short(__stdcall* GetAimedPlayerId)();
	const char* (__stdcall* GetPlayerName)(int);
	void(__stdcall* ShowDialog)(unsigned short, int, char*, char*, char*, char*);
	void(__stdcall* RegisterDialogCallback)(void(__stdcall*)(int, int, int, const char*));
};

typedef void(__stdcall* CSharpCommandCallback)(const char*);
typedef void(__stdcall* CSharpExportCallback)(const CSharpExports*);

static CSharpCommandCallback Foo;

static bool RegisterCallbacks() {
	const string managedModuleName = "ConsoleApp1.dll";
	const char* managedFunctionName = "Foo";

	char pathRaw[256];
	auto length = GetModuleFileName(SF->getSAMP()->getPluginInfo()->getPluginHandle(), pathRaw, 255);
	auto moduleFolder = string(pathRaw, length);
	auto modulePath = (path(moduleFolder).parent_path() / managedModuleName).string();

	HINSTANCE handle = LoadLibrary(modulePath.c_str());
	if (handle == NULL) return false;

	Foo = (CSharpCommandCallback)GetProcAddress(handle, managedFunctionName);
	if (Foo == NULL) return false;

	CSharpExportCallback exportCallback = (CSharpExportCallback)GetProcAddress(handle, "RegisterExports");
	if (exportCallback == NULL) return false;
	
	CSharpExports exports = {
		&SendChat,
		&LogToChat,
		&IsPlayerConnected,
		&GetAimedPlayerId,
		&GetPlayerName,
		&ShowDialog,
		&RegisterDialogCallback
	};
	exportCallback(&exports);

	return true;
}

static void CALLBACK OnBb(string args) {
	Foo(args.c_str());
}

static void CALLBACK mainloop() { 
	static bool initialized = false;
	if (!initialized && GAME && GAME->GetSystemState() == eSystemState::GS_PLAYING_GAME && SF->getSAMP()->IsInitialized()) {
		initialized = true;
		LogToChat(RegisterCallbacks() ? "C# callback resolved successfully." : "Failed to resolve C# callback.");
		SF->getSAMP()->registerChatCommand("test", &OnBb);
	}
}

bool PluginInit(HMODULE hModule) {
	SF = std::make_unique<SAMPFUNCS>();
	SF->initPlugin(&mainloop, hModule);
	return true;
}
