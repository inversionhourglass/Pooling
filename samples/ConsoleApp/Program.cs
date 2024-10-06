using ConsoleApp.Cases;

await SingletonPoolCase.TestAsync();

SemaphorePoolCase.Test();

ThreadLocalPoolCase.Test();

ServiceSetupPoolCase.Test();