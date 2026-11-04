// Menu klik kanan "Convert with Rotiva" untuk versi MSIX (Microsoft Store).
//
// Paket MSIX tidak bisa menulis menu Explorer ke registry (HKCU-nya divirtualisasi), jadi menu
// didaftarkan lewat <desktop4:FileExplorerContextMenus> di AppxManifest.xml. Explorer memuat DLL ini
// dan memanggil IExplorerCommand; Invoke menjalankan Rotiva lewat execution alias (supaya prosesnya
// tetap punya identitas paket) dengan "--wheel <file>...", lalu roda muncul di kursor.
//
// Build: installer\build-msix.ps1 (cl.exe dari Visual Studio Build Tools).

#include <windows.h>
#include <shlobj.h>
#include <shobjidl_core.h>
#include <shlwapi.h>
#include <new>
#include <string>
#include <vector>

// Sama dengan Clsid di installer\msix\AppxManifest.xml.
static const CLSID CLSID_ConvertCommand = { 0x345c3645, 0xd481, 0x40c5, { 0x90, 0xaa, 0xb3, 0x0d, 0x79, 0x8a, 0x46, 0xe1 } };

static HMODULE g_module;
static long g_objects;

// Batas panjang command line CreateProcess 32767; sisakan ruang. Pilihan sangat banyak dibagi ke
// beberapa proses, dan Rotiva menggabungkannya lagi (CommandChannel).
static const size_t MaxCommandLine = 30000;

static std::wstring ModuleDirectory()
{
    wchar_t path[MAX_PATH];
    DWORD length = GetModuleFileNameW(g_module, path, MAX_PATH);
    std::wstring result(path, length);
    return result.substr(0, result.find_last_of(L'\\'));
}

static std::wstring Quote(const std::wstring& text)
{
    // Aturan CommandLineToArgvW: backslash sebelum tanda kutip penutup harus digandakan.
    std::wstring result = L"\"";
    size_t backslashes = 0;
    for (wchar_t c : text)
    {
        if (c == L'\\')
        {
            backslashes++;
            continue;
        }
        if (c == L'"')
            result.append(backslashes * 2 + 1, L'\\');
        else
            result.append(backslashes, L'\\');
        backslashes = 0;
        result += c;
    }
    result.append(backslashes * 2, L'\\');
    return result + L"\"";
}

static bool Launch(const std::wstring& arguments)
{
    // Execution alias "Rotiva.exe" (AppxManifest.xml): menjalankan exe di folder paket langsung
    // menghasilkan proses tanpa identitas paket.
    PWSTR localAppData = nullptr;
    if (FAILED(SHGetKnownFolderPath(FOLDERID_LocalAppData, 0, nullptr, &localAppData)))
        return false;
    std::wstring alias = std::wstring(localAppData) + L"\\Microsoft\\WindowsApps\\Rotiva.exe";
    CoTaskMemFree(localAppData);

    std::wstring commandLine = Quote(alias) + L" --wheel" + arguments;
    STARTUPINFOW startup = { sizeof(startup) };
    PROCESS_INFORMATION process = {};
    if (!CreateProcessW(alias.c_str(), commandLine.data(), nullptr, nullptr, FALSE, 0, nullptr, nullptr, &startup, &process))
        return false;
    CloseHandle(process.hThread);
    CloseHandle(process.hProcess);
    return true;
}

class ConvertCommand final : public IExplorerCommand
{
    long _references = 1;

public:
    ConvertCommand() { InterlockedIncrement(&g_objects); }
    ~ConvertCommand() { InterlockedDecrement(&g_objects); }

    IFACEMETHODIMP QueryInterface(REFIID riid, void** object) override
    {
        static const QITAB table[] = { QITABENT(ConvertCommand, IExplorerCommand), { nullptr, 0 } };
        return QISearch(this, table, riid, object);
    }
    IFACEMETHODIMP_(ULONG) AddRef() override { return InterlockedIncrement(&_references); }
    IFACEMETHODIMP_(ULONG) Release() override
    {
        long references = InterlockedDecrement(&_references);
        if (references == 0)
            delete this;
        return references;
    }

    IFACEMETHODIMP GetTitle(IShellItemArray*, LPWSTR* name) override
    {
        // Bahasa tampilan Windows; pilihan bahasa di Rotiva tersimpan di registry paket yang tidak terlihat dari Explorer.
        bool indonesian = PRIMARYLANGID(GetUserDefaultUILanguage()) == LANG_INDONESIAN;
        return SHStrDupW(indonesian ? L"Konversi dengan Rotiva" : L"Convert with Rotiva", name);
    }

    IFACEMETHODIMP GetIcon(IShellItemArray*, LPWSTR* icon) override
    {
        return SHStrDupW((ModuleDirectory() + L"\\Rotiva.exe,0").c_str(), icon);
    }

    IFACEMETHODIMP GetToolTip(IShellItemArray*, LPWSTR* tip) override
    {
        *tip = nullptr;
        return E_NOTIMPL;
    }

    IFACEMETHODIMP GetCanonicalName(GUID* name) override
    {
        *name = CLSID_ConvertCommand;
        return S_OK;
    }

    IFACEMETHODIMP GetState(IShellItemArray*, BOOL, EXPCMDSTATE* state) override
    {
        *state = ECS_ENABLED;
        return S_OK;
    }

    IFACEMETHODIMP Invoke(IShellItemArray* items, IBindCtx*) override
    {
        if (items == nullptr)
            return E_INVALIDARG;
        DWORD count = 0;
        HRESULT hr = items->GetCount(&count);
        if (FAILED(hr))
            return hr;

        std::wstring arguments;
        for (DWORD i = 0; i < count; i++)
        {
            IShellItem* item = nullptr;
            if (FAILED(items->GetItemAt(i, &item)))
                continue;
            PWSTR path = nullptr;
            if (SUCCEEDED(item->GetDisplayName(SIGDN_FILESYSPATH, &path)))
            {
                std::wstring quoted = L" " + Quote(path);
                if (!arguments.empty() && arguments.size() + quoted.size() > MaxCommandLine)
                {
                    Launch(arguments);
                    arguments.clear();
                }
                arguments += quoted;
                CoTaskMemFree(path);
            }
            item->Release();
        }
        if (arguments.empty())
            return S_OK;
        return Launch(arguments) ? S_OK : HRESULT_FROM_WIN32(GetLastError());
    }

    IFACEMETHODIMP GetFlags(EXPCMDFLAGS* flags) override
    {
        *flags = ECF_DEFAULT;
        return S_OK;
    }

    IFACEMETHODIMP EnumSubCommands(IEnumExplorerCommand** commands) override
    {
        *commands = nullptr;
        return E_NOTIMPL;
    }
};

class ClassFactory final : public IClassFactory
{
    long _references = 1;

public:
    IFACEMETHODIMP QueryInterface(REFIID riid, void** object) override
    {
        static const QITAB table[] = { QITABENT(ClassFactory, IClassFactory), { nullptr, 0 } };
        return QISearch(this, table, riid, object);
    }
    IFACEMETHODIMP_(ULONG) AddRef() override { return InterlockedIncrement(&_references); }
    IFACEMETHODIMP_(ULONG) Release() override
    {
        long references = InterlockedDecrement(&_references);
        if (references == 0)
            delete this;
        return references;
    }

    IFACEMETHODIMP CreateInstance(IUnknown* outer, REFIID riid, void** object) override
    {
        *object = nullptr;
        if (outer != nullptr)
            return CLASS_E_NOAGGREGATION;
        auto* command = new (std::nothrow) ConvertCommand();
        if (command == nullptr)
            return E_OUTOFMEMORY;
        HRESULT hr = command->QueryInterface(riid, object);
        command->Release();
        return hr;
    }

    IFACEMETHODIMP LockServer(BOOL lock) override
    {
        if (lock)
            InterlockedIncrement(&g_objects);
        else
            InterlockedDecrement(&g_objects);
        return S_OK;
    }
};

BOOL APIENTRY DllMain(HMODULE module, DWORD reason, LPVOID)
{
    if (reason == DLL_PROCESS_ATTACH)
    {
        g_module = module;
        DisableThreadLibraryCalls(module);
    }
    return TRUE;
}

STDAPI DllGetClassObject(REFCLSID clsid, REFIID riid, void** object)
{
    *object = nullptr;
    if (clsid != CLSID_ConvertCommand)
        return CLASS_E_CLASSNOTAVAILABLE;
    auto* factory = new (std::nothrow) ClassFactory();
    if (factory == nullptr)
        return E_OUTOFMEMORY;
    HRESULT hr = factory->QueryInterface(riid, object);
    factory->Release();
    return hr;
}

STDAPI DllCanUnloadNow()
{
    return g_objects == 0 ? S_OK : S_FALSE;
}
