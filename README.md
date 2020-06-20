# SequentialScheduler
Defines a task scheduler that run tasks on a single thread.

# Source
This code mainly is from https://codereview.stackexchange.com/questions/43814/taskscheduler-that-uses-a-dedicated-thread. Additional code for passing thread is added.

# Use
To initialize an instance with a new free thread:
```
var scheduler = new SequentialScheduler();
```

To initialize an instance and pass a thread for running all tasks:
```
SequentialScheduler scheduler;
Thread myThread;

Main()
{
  //...
  
  scheduler = new SequentialScheduler(true);
  
  myThread = new Thread(MyThreadJob);
  myThread.Start();

  //...
}

MyThreadJob()
{
  //...
  
  scheduler.Run(); //This will block this thread until the scheduler disposed.
}
```

To run a task from this scheduler, like from any other scheduler:
```
var taskFactory = new TaskFactory(scheduler);
var result = taskFactory.StartNew(action);
```

**Note: This class is implemented from IDisposable. Don't forget to dispose it.**
