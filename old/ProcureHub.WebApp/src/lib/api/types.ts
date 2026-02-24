export type Result = {
  success: boolean;
  isPending: boolean;
};

export type ResultWithError<TError> = {
  success: boolean;
  isPending: boolean;
  error: TError;
};
