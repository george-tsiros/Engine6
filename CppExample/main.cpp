#include <Windows.h>
#include <gl/gl.h>
#define EX
HDC deviceContext;
HGLRC renderingContext;
DWORD lastError;
#ifdef EX
WNDCLASSEXW windowClass;
#else
WNDCLASS windowClass;
#endif
HWND windowHandle;

LRESULT CALLBACK WndProc(HWND hWnd,UINT message,WPARAM wParam,LPARAM lParam) {
    switch (message) {
    case WM_KEYUP:
        if (wParam==VK_ESCAPE)
            PostQuitMessage(0);
        break;
        //case WM_CREATE:
        //    deviceContext = GetDC(hWnd);
        //    if (renderingContext = wglCreateContext(deviceContext)) {
        //        if (!wglMakeCurrent(deviceContext,renderingContext)) {
        //            lastError = GetLastError();
        //            DebugBreak();
        //        }
        //    } else {
        //        lastError = GetLastError();
        //        DebugBreak();
        //    }
        //    break;
    case WM_PAINT:
    {
        //PAINTSTRUCT ps;
        glClearColor(0.5f,0.5f,0.5f,1.0f);
        //auto eh = wglGetProcAddress("glActiveTexture");
        //auto meh = GetProcAddress("glActiveTexture");
        //if (!eh) {
        //    lastError = GetLastError();
        //    DebugBreak();
        //}
        glClear(GL_COLOR_BUFFER_BIT|GL_DEPTH_BUFFER_BIT);
        SwapBuffers(deviceContext);
        //BeginPaint(hWnd,&ps);
        //EndPaint(hWnd,&ps);
    }
    break;
    case WM_CLOSE:
        wglMakeCurrent(NULL,NULL);
        wglDeleteContext(renderingContext);
        ReleaseDC(hWnd,deviceContext);
        PostQuitMessage(0);
        break;
    default:
        return DefWindowProc(hWnd,message,wParam,lParam);
    }
    return 0;
}

#ifdef EX
static void RegisterWindowClass(HINSTANCE instance) {
    windowClass = { 0 };
    windowClass.lpfnWndProc = WndProc;
    //windowClass.hInstance = instance;
    windowClass.lpszClassName = TEXT("PlainWindow");
    windowClass.style = CS_HREDRAW|CS_VREDRAW;

    windowClass.cbSize = sizeof(WNDCLASSEX);
    windowClass.hIcon = NULL;// LoadIcon(NULL,IDI_WINLOGO);
    windowClass.hCursor = NULL;//LoadCursor(NULL,IDC_ARROW);
    if (!RegisterClassExW(&windowClass)) {
        lastError = GetLastError();
        DebugBreak();
    }
}
#else
static void RegisterWindowClass(HINSTANCE instance) {
    windowClass = { 0 };
    windowClass.lpfnWndProc = WndProc;
    windowClass.hInstance = instance;
    windowClass.lpszClassName = TEXT("PlainWindow");
    windowClass.style = CS_OWNDC|CS_HREDRAW|CS_VREDRAW;
    if (!RegisterClass(&windowClass)) {
        lastError = GetLastError();
        DebugBreak();
    }
}
#endif

bool SetPixelFormat() {
    int pixelFormat;
    PIXELFORMATDESCRIPTOR pfd = { 0 };
    pfd.nSize = sizeof(PIXELFORMATDESCRIPTOR);
    pfd.nVersion = 1;
    pfd.dwFlags = PFD_DRAW_TO_WINDOW|PFD_SUPPORT_OPENGL|PFD_SUPPORT_COMPOSITION|PFD_DOUBLEBUFFER;
    pfd.iPixelType = PFD_TYPE_RGBA;
    pfd.cColorBits = 32;
    pfd.cAlphaBits = 8;
    pfd.cDepthBits = 32;
    pixelFormat = ChoosePixelFormat(deviceContext,&pfd);
    if (0==pixelFormat)
        return 0;
    return SetPixelFormat(deviceContext,pixelFormat,&pfd)!=0;
}

int WINAPI WinMain(__in HINSTANCE instance,__in_opt HINSTANCE previousInstance,__in LPSTR commandLine,__in int showCommand) {
    auto intSize = sizeof(int);
    auto longSize = sizeof(long);
    auto longlongSize = sizeof(long long);
    auto shortSize = sizeof(short);
    RegisterWindowClass(instance);
    DWORD windowStyle = WS_OVERLAPPEDWINDOW|WS_CLIPCHILDREN|WS_CLIPSIBLINGS;
    windowHandle = CreateWindow(windowClass.lpszClassName,windowClass.lpszClassName,windowStyle,0,0,640,480,0,0,NULL,0);
    deviceContext = GetDC(windowHandle);
    if (!SetPixelFormat()) {
        lastError = GetLastError();
        DebugBreak();
    }
    renderingContext = wglCreateContext(deviceContext);
    if (!renderingContext) {
        lastError = GetLastError();
        DebugBreak();
    }
    if (!wglMakeCurrent(deviceContext,renderingContext)) {
        lastError = GetLastError();
        DebugBreak();
    }
    ShowWindow(windowHandle,10);
    UpdateWindow(windowHandle);
    MSG msg;

    while (GetMessage(&msg,NULL,0,0)) {
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }

    UnregisterClass(windowClass.lpszClassName,NULL);

    return 0;
}
