TESTING 
2. Run the following command:

   ```shell
   dotnet tool install --global SKonsole
   ```
3. To confirm the installation was successful, run:
   ```shell
   skonsole --version
   ```

## Available Commands

- `skonsole commit <commitHash>`: Generate commit messages based on the provided commit hash.

- `skonsole pr feedback`: Generate valuable feedback for pull requests using git diff or git show output.

- `skonsole pr description`: Generate detailed descriptions for pull requests using git diff or git show output.

- `skonsole createPlan <message>`: Create plans using the Planner subcommand by providing a message.

- `skonsole promptChat`: Engage in interactive prompt chat sessions.

## Dependencies
This project requires the following dependencies:

- [Semantic Kernel](https://github.com/microsoft/semantic-kernel)

## Future Work
In the future, the SKonsole app could be expanded to support more skills and to parse arguments on launch. Additionally, the repository could include instructions for setting up NuGet credentials and using a GitHub Package source.

I hope this README is helpful for you and others who may use your repository in the future. Let me know if there's anything else I can do to help!


####_This README was generated using Semantic Kernel_
