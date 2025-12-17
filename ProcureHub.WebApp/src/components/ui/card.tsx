import * as React from 'react'

export function Card(props: React.HTMLAttributes<HTMLDivElement>) {
  const { className = '', ...rest } = props
  return (
    <div
      className={`rounded-xl border border-gray-200 bg-white p-6 shadow-sm dark:border-gray-800 dark:bg-gray-900 ${className}`}
      {...rest}
    />
  )
}

export function CardHeader(props: React.HTMLAttributes<HTMLDivElement>) {
  const { className = '', ...rest } = props
  return (
    <div className={`mb-4 space-y-1 ${className}`} {...rest} />
  )
}

export function CardTitle(props: React.HTMLAttributes<HTMLHeadingElement>) {
  const { className = '', ...rest } = props
  return (
    <h2
      className={`text-xl font-semibold leading-none tracking-tight ${className}`}
      {...rest}
    />
  )
}

export function CardDescription(
  props: React.HTMLAttributes<HTMLParagraphElement>,
) {
  const { className = '', ...rest } = props
  return (
    <p
      className={`text-sm text-gray-500 dark:text-gray-400 ${className}`}
      {...rest}
    />
  )
}

export function CardContent(props: React.HTMLAttributes<HTMLDivElement>) {
  const { className = '', ...rest } = props
  return <div className={`space-y-4 ${className}`} {...rest} />
}


